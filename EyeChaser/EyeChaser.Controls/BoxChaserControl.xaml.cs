using EyeChaser.Api;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    using Range1D = Tuple<double, double>;
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
                var skippedProbabilitySum = 0.0;
                foreach (IChaserQueryNode<Range1D> child in parent.Children)
                {
                    // Choose to display, according to whether there is enough of the node *within the screen bounds*
                    // (Note that if one edge is offscreen, we could skip over all remaining siblings?)
                    double onScreenProb = Math.Min(1.0, child.QueryCoords.Item2) - Math.Max(0.0, child.QueryCoords.Item1);
                    if (onScreenProb >= limit)
                    {
                        if (!HideSpaces && skippedProbabilitySum != 0)
                        {
                            var rowDefinitionExtra = new RowDefinition { Height = new GridLength(skippedProbabilitySum, GridUnitType.Star) };
                            TheGrid.RowDefinitions.Add(rowDefinitionExtra);
                            row++;
                        }

                        var rowDefinition = new RowDefinition { Height = new GridLength(onScreenProb, GridUnitType.Star) };
                        TheGrid.RowDefinitions.Add(rowDefinition);
                        // Scale by total size including any offscreen portion
                        var control = new BoxParentControl { Node = child, ProbabilityLimit = limit / (child.QueryCoords.Item2 - child.QueryCoords.Item1) };
                        Grid.SetRow(control, row);
                        TheGrid.Children.Add(control);
                        row++;
                        skippedProbabilitySum = 0.0;
                    }
                    else
                    {
                        skippedProbabilitySum += onScreenProb;
                    }
                }

                if (!HideSpaces && skippedProbabilitySum != 0)
                {
                    var rowDefinitionExtra = new RowDefinition { Height = new GridLength(skippedProbabilitySum, GridUnitType.Star) };
                    TheGrid.RowDefinitions.Add(rowDefinitionExtra);
                    row++;
                }
            }
        }
    }
}
