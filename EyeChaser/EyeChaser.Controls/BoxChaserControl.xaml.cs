using EyeChaser.Api;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxChildrenControl : UserControl
    {
        public readonly DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserNode), typeof(BoxChildrenControl),
            new PropertyMetadata(null, ParentNodeChanged));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxChildrenControl),
            new PropertyMetadata(0.01, ParentNodeChanged));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxChildrenControl),
            new PropertyMetadata(false));

        public BoxChildrenControl()
        {
            this.InitializeComponent();

            SizeChanged += (s, e) => DrawChildren();
        }

        public IChaserNode ParentNode
        {
            get { return (IChaserNode)GetValue(ParentNodeProperty); }
            set { SetValue(ParentNodeProperty, value); }
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

        static void ParentNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxChildrenControl)d;
            control.DrawChildren();
        }

        void DrawChildren()
        {
            TheCanvas.Children.Clear();

            var parent = ParentNode;

            var height = ActualHeight;

            if (parent != null && !double.IsNaN(height))
            {
                var limit = ProbabilityLimit;

                var cumulativeProbability = 0.0;
                foreach (IChaserNode child in parent)
                {
                    if (limit <= child.Probability)
                    {
                        var control = new BoxParentControl
                        {
                            Node = child,
                            ProbabilityLimit = limit / child.Probability,
                            Height = height * child.Probability
                        };

                        Canvas.SetTop(control, height * cumulativeProbability);

                        TheCanvas.Children.Add(control);
                    }

                    cumulativeProbability += child.Probability;
                }
            }
        }
    }
}
