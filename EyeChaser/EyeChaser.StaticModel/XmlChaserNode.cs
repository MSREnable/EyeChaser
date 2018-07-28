using EyeChaser.Api;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.StaticModel
{
    public class XmlChaserNode : IChaserNode<XmlChaserNode>
    {
        static readonly List<XmlChaserNode> _empty = new List<XmlChaserNode>();

        List<XmlChaserNode> _children;

        public XmlChaserNode Parent { get; private set; }

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

        public IEnumerator<XmlChaserNode> GetEnumerator()
        {
            return (_children ?? _empty).GetEnumerator();
        }

        public void Add(XmlChaserNode node)
        {
            if (_children == null)
            {
                _children = new List<XmlChaserNode>();
            }

            var limit = _children.Count;
            while (0 < limit && _children[limit - 1].Probability < node.Probability)
            {
                limit--;
            }
            _children.Insert(limit, node);

            node.Parent = this;
        }

        public Task RefreshChildrenAsync()
        {
            return Task.FromResult(0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(null, "Node", null);
            writer.WriteAttributeString(null, nameof(Caption), null, Caption);
            writer.WriteAttributeString(null, nameof(Probability), null, Probability.ToString());

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.WriteXml(writer);
                }
            }

            writer.WriteEndElement();
        }

        public async Task WriteXmlAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(null, "Node", null);
            await writer.WriteAttributeStringAsync(null, nameof(Caption), null, Caption);
            await writer.WriteAttributeStringAsync(null, nameof(Probability), null, Probability.ToString());

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    await child.WriteXmlAsync(writer);
                }
            }

            await writer.WriteEndElementAsync();
        }

        public static XmlChaserNode ReadXml(XmlReader reader)
        {
            while (!reader.IsStartElement("Node") && reader.Read()) ;

            var caption = reader.GetAttribute(nameof(Caption));
            var probabilityString = reader.GetAttribute(nameof(Probability));
            var probability = double.Parse(probabilityString);

            var root = new XmlChaserNode { Caption = caption, Probability = probability };

            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.IsStartElement("Node"))
                    {
                        var child = ReadXml(reader);
                        root.Add(child);
                    }
                }
                reader.Read();
            }

            return root;
        }

        static async Task<XmlChaserNode> ReadXmlNodeAsync(XmlReader reader)
        {
            while (!reader.IsStartElement("Node") && await reader.ReadAsync()) ;

            var caption = reader.GetAttribute(nameof(Caption));
            var probabilityString = reader.GetAttribute(nameof(Probability));
            var probability = double.Parse(probabilityString);

            var node = new XmlChaserNode { Caption = caption, Probability = probability };

            return node;
        }

        static async Task ReadXmlChildrenAsync(XmlReader reader, XmlChaserNode parent, XmlChaserNode root)
        {
            var isLeaf = true;

            if (reader.IsEmptyElement)
            {
                await reader.ReadAsync();
            }
            else
            {
                while (await reader.ReadAsync() && reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.IsStartElement("Node"))
                    {
                        var child = await ReadXmlNodeAsync(reader);
                        await ReadXmlChildrenAsync(reader, child, root);
                        parent.Add(child);

                        isLeaf = false;
                    }
                }
                await reader.ReadAsync();
            }

            if (isLeaf)
            {
                foreach (var child in root)
                {
                    parent.Add(child);
                }
            }
        }

        public static async Task<XmlChaserNode> ReadXmlAsync(XmlReader reader)
        {
            var root = await ReadXmlNodeAsync(reader);

            await ReadXmlChildrenAsync(reader, root, root);

            return root;
        }
    }
}
