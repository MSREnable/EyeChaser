using EyeChaser.Api;
using EyeChaser.Queries;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxRootControl : UserControl
    {
        public readonly static DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserQueryNode<Range1D>), typeof(BoxRootControl),
            new PropertyMetadata(null, ActualParentNodeChanged));

        public readonly static DependencyProperty EngineProperty = DependencyProperty.Register(nameof(Engine), typeof(IChaserQuery<Range1D>), typeof(BoxRootControl), new PropertyMetadata(null));

        public readonly static DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxRootControl),
            new PropertyMetadata(0.01, ParentNodeChanged));

        public readonly static DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxRootControl),
            new PropertyMetadata(false));

        public BoxRootControl()
        {
            this.InitializeComponent();

            SizeChanged += (s, e) => { var t = DrawChildrenAsync(); };
        }

        public IChaserQuery<Range1D> Engine
        {
            get { return (IChaserQuery<Range1D>)GetValue(EngineProperty); }
            set { SetValue(EngineProperty, value); }
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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            if (!e.Handled && 0 <= position.Y && position.Y <= ActualHeight)
            {
                var offset = (position.Y / ActualHeight);
                Engine.NavigateTo(new Range1D(offset, offset));

                e.Handled = true;
            }
            base.OnMouseDown(e);
        }

        async Task DrawChildrenAsync()
        {
            var parent = ParentNode;
            var height = ActualHeight;

            if (parent != null && !double.IsNaN(height))
            {
                var parentSpan = parent.QueryCoords;
                var parentSize = parentSpan.BoundSize;

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
                    double onScreenProb = parentSize * Math.Min(1.0, child.QueryCoords.UpperBound) - Math.Max(0.0, child.QueryCoords.LowerBound);
                    var startPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.LowerBound;
                    var endPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.UpperBound;
                    if (onScreenProb >= limit && 0 <= endPosition && startPosition <= 1)
                    {
                        while (currentChildControl != null && string.CompareOrdinal(currentChildControl.Node.Caption, child.Caption) < 0)
                        {
                            TheCanvas.Children.RemoveAt(childControlIndex);
                            currentChildControl = childControlIndex < TheCanvas.Children.Count ? (BoxParentControl)TheCanvas.Children[childControlIndex] : null;
                        }

                        BoxParentControl control;

                        double overallProb = endPosition - startPosition;

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

                        control.VisibleRange = new Range1D(Math.Max(0, -startPosition / overallProb), Math.Min(1, (1 - startPosition) / overallProb));
                        control.Width = ActualWidth;
                        control.ProbabilityLimit = limit / overallProb;
                        control.Height = height * overallProb;

                        Canvas.SetTop(control, height * startPosition);
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
