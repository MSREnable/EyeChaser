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

        public BoxChildrenControl()
        {
            this.InitializeComponent();
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

        static void ParentNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxChildrenControl)d;
            control.DrawChildren();
        }

        void DrawChildren()
        {
            TheGrid.RowDefinitions.Clear();
            TheGrid.Children.Clear();

            var parent = ParentNode;

            if (parent != null)
            {
                var limit = ProbabilityLimit;

                var row = 0;
                foreach (IChaserNode child in parent)
                {
                    if (limit <= child.Probability)
                    {
                        var rowDefinition = new RowDefinition { Height = new GridLength(child.Probability, GridUnitType.Star) };
                        TheGrid.RowDefinitions.Add(rowDefinition);

                        var control = new BoxParentControl { Node = child, ProbabilityLimit = limit / child.Probability };
                        Grid.SetRow(control, row);
                        TheGrid.Children.Add(control);
                        row++;
                    }
                }
            }
        }
    }
}
