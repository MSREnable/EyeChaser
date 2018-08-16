using EyeChaser.DynamicSwiftKeyModel;
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

            var root = SwiftKeyNode.CreateRoot();
            var sortedRoot = new AlphabeticChaserNode(root, 0.0);

            var engine = XmlChaserQueryEngine.Create(sortedRoot);

            BoxControl.Engine = engine;
            BoxControl.ParentNode = engine.Root;
        }
    }
}
