using EyeChaser.Api;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxChildrenControl : UserControl
    {
        public readonly DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserNode), typeof(BoxChildrenControl),
            new PropertyMetadata(null, ParentNodeChanged));

        public BoxChildrenControl()
        {
            this.InitializeComponent();
        }

        public IChaserNode ParentNode
        {
            get { return (IChaserNode)GetValue(ParentNodeProperty); }
            set { SetValue(ParentNodeProperty, value); }
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
                var row = 0;
                foreach (IChaserNode child in parent)
                {
                    var rowDefinition = new RowDefinition { Height = new GridLength(child.Probability, GridUnitType.Star) };
                    TheGrid.RowDefinitions.Add(rowDefinition);

                    var control = new BoxParentControl { Node = child };
                    Grid.SetRow(control, row);
                    TheGrid.Children.Add(control);
                    row++;
                }
            }
        }
    }
}
