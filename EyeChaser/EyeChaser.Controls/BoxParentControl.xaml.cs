using EyeChaser.Api;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxParentControl : UserControl
    {
        public readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(IChaserNode), typeof(BoxParentControl),
            new PropertyMetadata(null));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxParentControl),
            new PropertyMetadata(0.01));

        public BoxParentControl()
        {
            this.InitializeComponent();
        }

        public IChaserNode Node
        {
            get { return (IChaserNode)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        public double ProbabilityLimit
        {
            get { return (double)GetValue(ProbabilityLimitProperty); }
            set { SetValue(ProbabilityLimitProperty, value); }
        }
    }
}
