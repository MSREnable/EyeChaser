using EyeChaser.Api;
using EyeChaser.Queries;
using System.Windows;
using System.Windows.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxParentControl : UserControl
    {
        public readonly static DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(IChaserQueryNode<Range1D>), typeof(BoxParentControl),
            new PropertyMetadata(null));

        public readonly static DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxParentControl),
            new PropertyMetadata(double.PositiveInfinity, ProbabilityLimitChanged));

        public readonly static DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxParentControl),
            new PropertyMetadata(false));

        public readonly static DependencyProperty VisibleRangeProperty = DependencyProperty.Register(nameof(VisibleRange), typeof(Range1D), typeof(BoxParentControl),
            new PropertyMetadata(new Range1D(0, 1), VisibleRangeChanged));

        static void VisibleRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxParentControl)d;
            control.TheChildren.VisibleRange = control.VisibleRange;
        }

        static void ProbabilityLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxParentControl)d;
            control.TheChildren.ProbabilityLimit = control.ProbabilityLimit;
        }

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

        public Range1D VisibleRange
        {
            get { return (Range1D)GetValue(VisibleRangeProperty); }
            set { SetValue(VisibleRangeProperty, value); }
        }
    }
}
