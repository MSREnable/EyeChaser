using EyeChaser.Api;
using EyeChaser.StaticModel;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxChildrenControl2D : UserControl
    {
        public readonly DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserQueryNode<Rect2D>), typeof(BoxChildrenControl),
            new PropertyMetadata(null, ParentNodeChanged));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxChildrenControl),
            new PropertyMetadata(0.01, ParentNodeChanged));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxChildrenControl),
            new PropertyMetadata(false));

        public BoxChildrenControl2D()
        {
            this.InitializeComponent();

            SizeChanged += (s, e) => { var t = DrawChildrenAsync(); };
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
            var control = (BoxChildrenControl2D)d;
            var t = control.DrawChildrenAsync();
        }

        async Task DrawChildrenAsync()
        {
            TheCanvas.Children.Clear();

            var parent = ParentNode;

            var height = ActualHeight;
            var width = ActualWidth;

            if (parent != null && !double.IsNaN(height))
            {
                var limit = ProbabilityLimit;

                if (parent.IsUpdateNeeded)
                {
                    await parent.UpdateAsync();
                }

                foreach (IChaserQueryNode<Rect2D> child in parent.Children)
                {
                    // Choose to display, according to whether there is enough of the node *within the screen bounds*
                    double onScreenProb = (Math.Min(1.0, child.QueryCoords.Right) - Math.Max(0.0, child.QueryCoords.Left))
                        * (Math.Min(1.0, child.QueryCoords.Bottom) - Math.Max(0.0, child.QueryCoords.Top));
                    if (onScreenProb >= limit)
                    {
                        double childHeight = child.QueryCoords.Height;
                        double childWidth = child.QueryCoords.Width;

                        UIElement control = /*(child.Children.Count==0) 
                        ? (UIElement)new TextBlock
                        {
                            Text = child.Caption
                        }
                        :*/ new BoxParentControl2D
                        {
                            Node = child,
                            ProbabilityLimit = limit / (childHeight * childWidth), //Not right as doesn't take into account BoxParentControl2D's internal border
                            Height = height * childHeight,
                            Width = width * childWidth
                        };

                        Canvas.SetTop(control, height * child.QueryCoords.Top);
                        Canvas.SetLeft(control, width * child.QueryCoords.Left);

                        TheCanvas.Children.Add(control);
                    }
                }
            }
        }
    }
}
