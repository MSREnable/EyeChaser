using EyeChaser.Api;
using EyeChaser.Queries;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxTileControl : UserControl
    {
        public readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(XmlChaserQueryNode2D), typeof(BoxTileControl),
            new PropertyMetadata(null));

        public readonly DependencyProperty TileColorProperty = DependencyProperty.Register(nameof(TileColor), typeof(Brush), typeof(BoxTileControl),
            new PropertyMetadata(null));
        
        public BoxTileControl()
        {
            this.InitializeComponent();
        }

        public XmlChaserQueryNode2D Node
        {
            get { return (XmlChaserQueryNode2D)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        public Brush TileColor
        {
            get { return (Brush)GetValue(TileColorProperty); }
            set { SetValue(TileColorProperty, value); }
        }

        private void TextBlock_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            string u = Node.Uri;
            if (u != "")
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri(u));
            }
        }
    }
}
