using EyeChaser.Api;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.StaticModel
{
    public class XmlTileNode : IChaserNode<XmlTileNode>
    {
        readonly List<XmlTileNode> _children = new List<XmlTileNode>();

        public XmlTileNode Parent { get; private set; }

        public string Caption { get; set; }

        public double Probability { get; set; }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { }
            remove { }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { }
            remove { }
        }

        public bool IsChildrenPopulated { get; private set; }

        public IEnumerator<XmlTileNode> GetEnumerator() => _children.GetEnumerator();

        public Task RefreshChildrenAsync() => Task.FromResult(0);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Rect2D prespecifiedCoords { get; private set; }

        public string Uri { get; private set; }

        public string ImageFile { get; private set; }

        public static async Task<XmlTileNode> ReadXmlAsync(XmlReader reader, XmlTileNode parent = null)
        {
            while (!reader.IsStartElement("Node") && !reader.IsStartElement("Group") && await reader.ReadAsync()) ;

            var caption = reader.GetAttribute(nameof(Caption)) ?? "";
            var left = double.Parse(reader.GetAttribute(nameof(Rect2D.Left)));
            var right = double.Parse(reader.GetAttribute(nameof(Rect2D.Right)));
            var top = double.Parse(reader.GetAttribute(nameof(Rect2D.Top)));
            var bottom = double.Parse(reader.GetAttribute(nameof(Rect2D.Bottom)));
            var uri = reader.GetAttribute(nameof(Uri)) ?? "";
            var image = reader.GetAttribute(nameof(ImageFile)) ?? "";

            var root = new XmlTileNode {
                Caption = caption,
                prespecifiedCoords = new Rect2D(left, right, top, bottom),
                Uri = uri,
                ImageFile = image
            };

            if (reader.IsEmptyElement)
            {
                await reader.ReadAsync();
            }
            else
            {
                while (await reader.ReadAsync() && reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.IsStartElement("Node") || reader.IsStartElement("Group"))
                    {
                        var child = await ReadXmlAsync(reader);
                        root._children.Add(child);
                    }
                }
                await reader.ReadAsync();
            }

            return root;
        }
    }
}
