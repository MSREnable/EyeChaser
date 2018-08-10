using EyeChaser.Model;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace EyeChaser.WpfShell
{
    /// <summary>
    /// Interaction logic for ChaserChildrenControl.xaml
    /// </summary>
    public partial class ChaserChildrenControl : UserControl
    {
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(nameof(Children), typeof(ReadOnlyObservableCollection<ModelNode>), typeof(ChaserChildrenControl),
            new PropertyMetadata(null, OnChildrenChanged));

        ReadOnlyObservableCollection<ModelNode> _children;

        public ChaserChildrenControl()
        {
            InitializeComponent();
        }

        public ReadOnlyObservableCollection<ModelNode> Children
        {
            get { return (ReadOnlyObservableCollection<ModelNode>)GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }

        static void OnChildrenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ChaserChildrenControl)d;

            if (control._children != null)
            {
                INotifyCollectionChanged collection = control._children;
                collection.CollectionChanged -= control.OnCollectionChanged;
            }

            control._children = control.Children;

            if (control._children != null)
            {
                INotifyCollectionChanged collection = control._children;
                collection.CollectionChanged += control.OnCollectionChanged;
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TheGrid.Children.Clear();
            TheGrid.RowDefinitions.Clear();

            var limit = 0.0;
            var rowCount = 0;
            foreach (var child in _children)
            {
                if (limit < child.VisibleRange.LowerBound)
                {
                    TheGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(child.VisibleRange.LowerBound - limit, GridUnitType.Star) });
                    rowCount++;
                }

                TheGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(child.BoundSize, GridUnitType.Star) });

            }
        }
    }
}
