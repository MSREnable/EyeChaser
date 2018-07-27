using EyeChaser.Queries;
using EyeChaser.StaticModel;
using System.IO;
using System.Windows;
using System.Xml;

namespace EyeChaser.TestShell.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var stream = new StringReader(Properties.Resources.MockData);
            var settings = new XmlReaderSettings { Async = true };
            var reader = XmlReader.Create(stream, settings);

            var xmlRoot = await XmlChaserNode.ReadXmlAsync(reader);
            var sortedRoot = new AlphabeticChaserNode(xmlRoot, 0.05);

            var engine = XmlChaserQueryEngine.Create(sortedRoot);

            BoxControl.Engine = engine;
            BoxControl.ParentNode = engine.Root;
        }
    }
}
