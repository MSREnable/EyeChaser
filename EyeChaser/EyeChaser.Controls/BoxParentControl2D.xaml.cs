using EyeChaser.Api;
using EyeChaser.StaticModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxParentControl2D : UserControl
    {
        public readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(IChaserQueryNode<Rect2D>), typeof(BoxParentControl),
            new PropertyMetadata(null));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxParentControl),
            new PropertyMetadata(0.01));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxChildrenControl),
            new PropertyMetadata(false));

        public BoxParentControl2D()
        {
            this.InitializeComponent();
        }

        public IChaserQueryNode<Rect2D> Node
        {
            get { return (IChaserQueryNode<Rect2D>)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        public double ProbabilityLimit
        {
            get { return (double)GetValue(ProbabilityLimitProperty); }
            set { SetValue(ProbabilityLimitProperty, value); }
        }

        public bool HideSpaces
        {
            get { return (bool)GetValue(HideSpacesProperty); }
            set { SetValue(HideSpacesProperty, value); }
        }
    }
}
