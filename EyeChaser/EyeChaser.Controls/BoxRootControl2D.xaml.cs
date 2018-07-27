using EyeChaser.Api;
using EyeChaser.Queries;
using EyeChaser.StaticModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxRootControl2D : UserControl
    {
        public readonly DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserQueryNode<Rect2D>), typeof(BoxRootControl2D),
            new PropertyMetadata(null, ActualParentNodeChanged));

        public readonly DependencyProperty EngineProperty = DependencyProperty.Register(nameof(Engine), typeof(IChaserQuery<Rect2D>), typeof(BoxRootControl2D), new PropertyMetadata(null));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxRootControl2D),
            new PropertyMetadata(0.01, ParentNodeChanged));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxRootControl2D),
            new PropertyMetadata(false));

        public BoxRootControl2D()
        {
            this.InitializeComponent();

            SizeChanged += (s, e) => { var t = DrawChildrenAsync(); };
        }

        public IChaserQuery<Rect2D> Engine
        {
            get { return (IChaserQuery<Rect2D>)GetValue(EngineProperty); }
            set { SetValue(EngineProperty, value); }
        }

        public IChaserQueryNode<Rect2D> ParentNode
        {
            get { return (IChaserQueryNode<Rect2D>)GetValue(ParentNodeProperty); }
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
            var control = (BoxRootControl2D)d;
            var t = control.DrawChildrenAsync();
        }

        static void ActualParentNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ParentNodeChanged(d, e);

            var control = (BoxRootControl2D)d;
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
            if (e.PropertyName == null || e.PropertyName == nameof(IChaserQueryNode<Rect2D>.Children))
            {
                await DrawChildrenAsync();
            }
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(this).Position;
            if (!e.Handled && 0 <= position.X && position.X <= ActualWidth && 0 <= position.Y && position.Y <= ActualHeight )
            {
                var offsetX = (position.X / ActualWidth);
                var offsetY = (position.Y / ActualHeight);
                Engine.NavigateTo(new Rect2D(offsetX, offsetX, offsetY, offsetY));

                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }

        async Task DrawChildrenAsync()
        {
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

                var childControlIndex = 0;
                var currentChildControl = childControlIndex < TheCanvas.Children.Count ? (BoxParentControl2D)TheCanvas.Children[childControlIndex] : null;

                foreach (IChaserQueryNode<Rect2D> child in parent.Children)
                {
                    // Choose to display, according to whether there is enough of the node *within the screen bounds*
                    // (Note that if one edge is offscreen, we could skip over all remaining siblings?)
                    double onScreenProb = parentSize * Math.Min(1.0, child.QueryCoords.UpperBound) - Math.Max(0.0, child.QueryCoords.LowerBound);
                    var startPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.LowerBound;
                    var endPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.UpperBound;
                    if (onScreenProb >= limit && 0 <= endPosition && startPosition <= 1)
                    {
                        if (currentChildControl != null && string.CompareOrdinal(currentChildControl.Node.Caption, child.Caption) < 0)
                        {
                            TheCanvas.Children.RemoveAt(childControlIndex);
                            currentChildControl = childControlIndex < TheCanvas.Children.Count ? (BoxParentControl2D)TheCanvas.Children[childControlIndex] : null;
                        }

                        BoxParentControl2D control;

                        double overallProb = endPosition - startPosition;

                        if (currentChildControl == null || string.CompareOrdinal(currentChildControl.Node.Caption, child.Caption) != 0)
                        {
                            control = new BoxParentControl2D
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
                            currentChildControl = childControlIndex < TheCanvas.Children.Count ? (BoxParentControl2D)TheCanvas.Children[childControlIndex] : null;
                        }

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
