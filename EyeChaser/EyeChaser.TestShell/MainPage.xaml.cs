using EyeChaser.StaticModel;
using EyeChaser.Transforms;
using System;
using System.IO;
using System.Xml;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EyeChaser.TestShell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var InstallationFolder = Package.Current.InstalledLocation;
            var file = await InstallationFolder.GetFileAsync(@"Assets\MockData.xml");
            var stream = await file.OpenStreamForReadAsync();
            var settings = new XmlReaderSettings { Async = true };
            var reader = XmlReader.Create(stream, settings);
            var root = await XmlChaserNode.ReadXmlAsync(reader);

            BoxControl.ParentNode = new AlphabeticChaserNode(root, 0.02);
        }
    }
}
