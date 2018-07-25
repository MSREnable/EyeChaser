using EyeChaser.Api;
using EyeChaser.Queries;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxParentControl : UserControl
    {
        public readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(IChaserQueryNode<Range1D>), typeof(BoxParentControl),
            new PropertyMetadata(null));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxParentControl),
            new PropertyMetadata(0.01));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxChildrenControl),
            new PropertyMetadata(false));

        public BoxParentControl()
        {
            this.InitializeComponent();
        }

        public IChaserQueryNode<Range1D> Node
        {
            get { return (IChaserQueryNode<Range1D>)GetValue(NodeProperty); }
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

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(this).Position;
            if (!e.Handled && 0 <= position.Y && position.Y <= ActualHeight)
            {
                var offset = position.Y / ActualHeight;
                Node.NavigateTo(new Range1D(offset, offset));

                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }
    }
}
