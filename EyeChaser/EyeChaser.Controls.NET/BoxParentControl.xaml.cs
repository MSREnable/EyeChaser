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

        public readonly static DependencyProperty TextMarginProperty = DependencyProperty.Register(nameof(TextMargin), typeof(Thickness), typeof(BoxParentControl), new PropertyMetadata(new Thickness(0), TextMarginChanged));

        public Thickness TextMargin
        {
            get => (Thickness)GetValue(TextMarginProperty);
            // Set only in MySizeChanged as there is only one legal value
        }

        static void VisibleRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxParentControl)d;
            control.TheChildren.VisibleRange = control.VisibleRange;
            MySizeChanged(control, null); // Update TextMargin
        }

        static void ProbabilityLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxParentControl)d;
            control.TheChildren.ProbabilityLimit = control.ProbabilityLimit;
        }

        static void TextMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxParentControl)d;
            control.TextWrapper.Margin = control.TextMargin;
        }

        public BoxParentControl()
        {
            this.InitializeComponent();
            SizeChanged += new SizeChangedEventHandler(MySizeChanged);
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

        private static void MySizeChanged(object sender, SizeChangedEventArgs e)
        {
            var control = (BoxParentControl)sender;
            if (double.IsNaN(control.Height)) return;
            var t = new Thickness(0, control.Height * control.VisibleRange.LowerBound, 0, control.Height * (1.0 - control.VisibleRange.UpperBound));
            control.SetValue(TextMarginProperty, t);
        }
    }
}
