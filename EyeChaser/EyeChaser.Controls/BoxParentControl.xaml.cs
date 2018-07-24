using EyeChaser.Api;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    using Range1D = Tuple<double, double>;

    public sealed partial class BoxParentControl : UserControl
    {
        public readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(IChaserNode), typeof(BoxParentControl),
            new PropertyMetadata(null));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxParentControl),
            new PropertyMetadata(0.01));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxChildrenControl),
            new PropertyMetadata(false));

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
    }
}
