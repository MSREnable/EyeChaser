using EyeChaser.Api;
using EyeChaser.Queries;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
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

        async Task DrawChildrenAsync()
        {
            var parent = ParentNode;

            var height = ActualHeight;

            if (parent != null && !double.IsNaN(height))
            {
                var limit = ProbabilityLimit;

                if (parent.IsUpdateNeeded)
                {
                    await parent.UpdateAsync();
                }

                var childControlIndex = 0;
                var currentChildControl = childControlIndex < TheCanvas.Children.Count ? (BoxParentControl)TheCanvas.Children[childControlIndex] : null;

                foreach (IChaserQueryNode<Range1D> child in parent.Children)
                {
                    // Choose to display, according to whether there is enough of the node *within the screen bounds*
                    // (Note that if one edge is offscreen, we could skip over all remaining siblings?)
                    double onScreenProb = Math.Min(1.0, child.QueryCoords.UpperBound) - Math.Max(0.0, child.QueryCoords.LowerBound);
                    if (onScreenProb >= limit)
                    {
                        if (currentChildControl != null && string.CompareOrdinal(currentChildControl.Node.Caption, child.Caption) < 0)
                        {
                            TheCanvas.Children.RemoveAt(childControlIndex);
                            currentChildControl = childControlIndex < TheCanvas.Children.Count ? (BoxParentControl)TheCanvas.Children[childControlIndex] : null;
                        }

                        BoxParentControl control;

                        double overallProb = child.QueryCoords.BoundSize;

                        if (currentChildControl == null || string.CompareOrdinal(currentChildControl.Node.Caption, child.Caption) != 0)
                        {
                            control = new BoxParentControl
                            {
                                Node = child
                            };

                            TheCanvas.Children.Insert(childControlIndex, control);

                            childControlIndex++;
                        }
                        else
                        {
                            control = currentChildControl;

                            childControlIndex++;
                            currentChildControl = childControlIndex < TheCanvas.Children.Count ? (BoxParentControl)TheCanvas.Children[childControlIndex] : null;
                        }

                        control.Width = ActualWidth;
                        control.ProbabilityLimit = limit / overallProb;
                        control.Height = height * overallProb;

                        Canvas.SetTop(control, height * child.QueryCoords.LowerBound);
                    }
                }

                while (childControlIndex < TheCanvas.Children.Count)
                {
                    TheCanvas.Children.RemoveAt(TheCanvas.Children.Count - 1);
                }
            }
        }
    }
}
