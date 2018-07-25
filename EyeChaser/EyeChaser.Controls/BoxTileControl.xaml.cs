﻿using EyeChaser.Api;
using EyeChaser.StaticModel;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeChaser.Controls
{
    public sealed partial class BoxTileControl : UserControl
    {
        public readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(XmlTileNode), typeof(BoxTileControl),
            new PropertyMetadata(null));

        public readonly DependencyProperty TileColorProperty = DependencyProperty.Register(nameof(TileColor), typeof(SolidColorBrush), typeof(BoxTileControl),
            new PropertyMetadata(null));
        
        public BoxTileControl()
        {
            this.InitializeComponent();
        }

        public XmlTileNode Node
        {
            get { return (XmlTileNode)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        public SolidColorBrush TileColor
        {
            get { return (SolidColorBrush)GetValue(TileColorProperty); }
            set { SetValue(TileColorProperty, value); }
        }

        private void TextBlock_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            string u = Node.Uri;
            if (u != "")
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri(u));
            }
        }
    }
}
