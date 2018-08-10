using EyeChaser.Model;
using EyeChaser.Queries;
using EyeChaser.WpfShell.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace EyeChaser.WpfShell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var xml = Properties.Resources.MockData;
            var stream = new StringReader(xml);
            //var installationFolder = Package.Current.InstalledLocation;
            //var file = await installationFolder.GetFileAsync(@"Assets\MockData.xml");
            //var stream = await file.OpenStreamForReadAsync();
            var settings = new XmlReaderSettings { Async = true };
            var reader = XmlReader.Create(stream, settings);

            var engine = await XmlChaserQueryEngine.CreateAsync(reader);
            var root = new RootModel(engine.Root);
            await root.UpdateAsync(0.01);
        }
    }
}
