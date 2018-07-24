using EyeChaser.Api;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.StaticModel
{
    public class XmlTileNode : IChaserQueryNode<Rect2D>
    {
        public int Generation => throw new System.NotImplementedException();

        public IChaserQueryNode<Rect2D> Parent { get; private set; }

        public string Caption { get; private set; }

        public string Uri { get; private set; }

        private readonly List<XmlTileNode> _children = new List<XmlTileNode>();

        public IReadOnlyList<IChaserQueryNode<Rect2D>> Children
        {
            get { return _children; }
        }

        public Rect2D QueryCoords { get; private set; }

        public bool IsUpdateNeeded => false;

        public Task UpdateAsync() => Task.FromResult(false);

        public static async Task<XmlTileNode> ReadXmlAsync(XmlReader reader)
        {
            while (!reader.IsStartElement("Node") && !reader.IsStartElement("Group") && await reader.ReadAsync()) ;

            var caption = reader.GetAttribute(nameof(Caption));
            var left = double.Parse(reader.GetAttribute(nameof(Rect2D.Left)));
            var right = double.Parse(reader.GetAttribute(nameof(Rect2D.Right)));
            var top = double.Parse(reader.GetAttribute(nameof(Rect2D.Top)));
            var bottom = double.Parse(reader.GetAttribute(nameof(Rect2D.Bottom)));
            var uri = reader.GetAttribute(nameof(Uri));

            var root = new XmlTileNode { Caption = caption, QueryCoords = new Rect2D(left, right, top, bottom), Uri = uri ?? "" };

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
