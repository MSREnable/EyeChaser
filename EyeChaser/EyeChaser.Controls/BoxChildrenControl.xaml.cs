using EyeChaser.Api;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    using Range1D = Tuple<double, double>;

    public sealed partial class BoxChildrenControl : UserControl
    {
        public readonly DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserQueryNode<Range1D>), typeof(BoxChildrenControl),
            new PropertyMetadata(null, ParentNodeChanged));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxChildrenControl),
            new PropertyMetadata(0.01, ParentNodeChanged));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxChildrenControl),
            new PropertyMetadata(false));

        public BoxChildrenControl()
        {
            this.InitializeComponent();

            SizeChanged += (s, e) => { var t = DrawChildrenAsync(); };
        }

        public IChaserQueryNode<Range1D> ParentNode
        {
            get { return (IChaserQueryNode<Range1D>)GetValue(ParentNodeProperty); }
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
            var t = control.DrawChildrenAsync();
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(this).Position;
            if (!e.Handled && 0 <= position.Y && position.Y <= ActualHeight)
            {
                var offset = position.Y / ActualHeight;
                ParentNode.NavigateTo(new Range1D(offset, offset));

                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }

        async Task DrawChildrenAsync()
        {
            TheCanvas.Children.Clear();

            var parent = ParentNode;

            var height = ActualHeight;

            if (parent != null && !double.IsNaN(height))
            {
                var limit = ProbabilityLimit;

                if (parent.IsUpdateNeeded)
                {
                    await parent.UpdateAsync();
                }

                foreach (IChaserQueryNode<Range1D> child in parent.Children)
                {
                    // Choose to display, according to whether there is enough of the node *within the screen bounds*
                    // (Note that if one edge is offscreen, we could skip over all remaining siblings?)
                    double onScreenProb = Math.Min(1.0, child.QueryCoords.Item2) - Math.Max(0.0, child.QueryCoords.Item1);
                    if (onScreenProb >= limit)
                    {
                        double overallProb = child.QueryCoords.Item2 - child.QueryCoords.Item1;
                        var control = new BoxParentControl
                        {
                            Width = ActualWidth,
                            Node = child,
                            ProbabilityLimit = limit / overallProb,
                            Height = height * overallProb
                        };

                        Canvas.SetTop(control, height * child.QueryCoords.Item1);

                        TheCanvas.Children.Add(control);
                    }
                }
            }
        }
    }
}
