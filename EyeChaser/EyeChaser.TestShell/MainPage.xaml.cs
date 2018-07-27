using EyeChaser.Queries;
using EyeChaser.SwiftKeyModel;
using EyeChaser.Transforms;
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
            var swiftKeyRoot = SwiftKeyNode.CreateRoot();
            var sortedRoot = new AlphabeticChaserNode(swiftKeyRoot, 0.05);

            var engine = XmlChaserQueryEngine.Create(sortedRoot);

            BoxControl.Engine = engine;
            BoxControl.ParentNode = engine.Root;
        }
    }
}
