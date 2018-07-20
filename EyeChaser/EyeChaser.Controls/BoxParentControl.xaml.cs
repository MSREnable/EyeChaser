using EyeChaser.Api;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxParentControl : UserControl
    {
        public readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(IChaserNode), typeof(BoxChildrenControl),
            new PropertyMetadata(null));

        public BoxParentControl()
        {
            this.InitializeComponent();
        }

        public IChaserNode Node
        {
            get { return (IChaserNode)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }
    }
}
