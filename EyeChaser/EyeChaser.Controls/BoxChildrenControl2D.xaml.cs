using EyeChaser.Api;
using EyeChaser.StaticModel;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxChildrenControl2D : UserControl
    {
        public readonly DependencyProperty ParentNodeProperty = DependencyProperty.Register(nameof(ParentNode), typeof(IChaserQueryNode<Rect2D>), typeof(BoxChildrenControl2D),
            new PropertyMetadata(null, ParentNodeChanged));

        public readonly DependencyProperty ProbabilityLimitProperty = DependencyProperty.Register(nameof(ProbabilityLimit), typeof(double), typeof(BoxChildrenControl2D),
            new PropertyMetadata(0.01, ParentNodeChanged));

        public readonly DependencyProperty HideSpacesProperty = DependencyProperty.Register(nameof(HideSpaces), typeof(bool), typeof(BoxChildrenControl2D),
            new PropertyMetadata(false));

        public static readonly FontFamily SegoeFont = new FontFamily("Segoe MDL2 Assets");
        public static readonly FontFamily ScriptFont = new FontFamily("MV Boli");
        public static readonly Thickness MyBorderThickness = new Thickness(2.0, 2.0, 2.0, 2.0);
        public static readonly Brush MyBorderBrush = new SolidColorBrush { Color = new Windows.UI.Color { R = 0, G = 0xee, B = 0xee } };
        
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
                        //var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                        //var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                        //sysicon.Dispose();
                        FrameworkElement control;
                        if (child.Children.Count == 0)
                        {
                            int captionHash = child.Caption.GetHashCode();
                            Windows.UI.Color c = new Windows.UI.Color { A=0xff, R=(byte)captionHash, G=(byte)(captionHash >> 8), B = (byte)(captionHash >> 16) };
                            control = new BoxTileControl
                            {
                                Node = (XmlTileNode)child,
                                FontFamily = (child.Caption.Length > 1) ? ScriptFont : SegoeFont,
                                FontSize = 36 - child.Caption.Length,
                                TileColor = new SolidColorBrush(c)
                            };
                        }
                        else
                        {
                            control = new BoxParentControl2D
                            {
                                Node = child,
                                ProbabilityLimit = limit / (childHeight * childWidth), //Not right as doesn't take into account BoxParentControl2D's internal border
                                //Height = height * childHeight,
                                //Width = width * childWidth
                            };
                        }
                        control.Height = height * childHeight;
                        control.Width = width * childWidth;

                        Canvas.SetTop(control, height * child.QueryCoords.Top);
                        Canvas.SetLeft(control, width * child.QueryCoords.Left);

                        TheCanvas.Children.Add(control);
                    }
                }
            }
        }
    }
}
