using EyeChaser.Api;
using EyeChaser.Queries;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxRootControl : UserControl
    {
        public readonly DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserQueryNode<Range1D>), typeof(BoxRootControl),
            new PropertyMetadata(null, ActualParentNodeChanged));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxRootControl),
            new PropertyMetadata(0.01, ParentNodeChanged));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxRootControl),
            new PropertyMetadata(false));

        public BoxRootControl()
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
            var control = (BoxRootControl)d;
            var t = control.DrawChildrenAsync();
        }

        static void ActualParentNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ParentNodeChanged(d, e);

            var control = (BoxRootControl)d;
            var t = control.DrawChildrenAsync();

            var oldParent = e.OldValue as INotifyPropertyChanged;
            var newParent = e.NewValue as INotifyPropertyChanged;

            if (oldParent != null)
            {
                oldParent.PropertyChanged -= control.ParentPropertyChanged;
            }

            if (newParent != null)
            {
                newParent.PropertyChanged += control.ParentPropertyChanged;
            }
        }

        async void ParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null || e.PropertyName == nameof(IChaserQueryNode<Range1D>.Children))
            {
                await DrawChildrenAsync();
            }
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(this).Position;
            if (!e.Handled && 0 <= position.Y && position.Y <= ActualHeight)
            {
                // Compute the inverse transform that NavigateTo will apply
                var desiredNavigateCenter = (position.Y / ActualHeight);
                var offset = (desiredNavigateCenter - this.ParentNode.QueryCoords.LowerBound) / this.ParentNode.QueryCoords.BoundSize;
                ParentNode.NavigateTo(new Range1D(offset, offset));

                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }

        async Task DrawChildrenAsync()
        {
            TheCanvas.Children.Clear();

            var parent = ParentNode;
            var parentSpan = parent.QueryCoords;
            var parentSize = parentSpan.BoundSize;

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
                    double onScreenProb = parentSize * Math.Min(1.0, child.QueryCoords.UpperBound) - Math.Max(0.0, child.QueryCoords.LowerBound);
                    if (onScreenProb >= limit)
                    {
                        var startPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.LowerBound;
                        var endPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.UpperBound;

                        double overallProb = endPosition - startPosition;
                        var control = new BoxParentControl
                        {
                            Width = ActualWidth,
                            Node = child,
                            ProbabilityLimit = limit / overallProb,
                            Height = height * overallProb
                        };

                        Canvas.SetTop(control, height * startPosition);

                        TheCanvas.Children.Add(control);
                    }
                }
            }
        }
    }
}
