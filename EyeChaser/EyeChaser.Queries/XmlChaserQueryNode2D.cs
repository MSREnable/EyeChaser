using EyeChaser.Api;
using EyeChaser.StaticModel;
using EyeChaser.Transforms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.Queries
{
    public class XmlChaserQueryNode2D : ChaserQueryNode<Rect2D>
    {
        public string Uri => ((XmlTileNode)this._node).Uri;
        public string ImageFile => ((XmlTileNode)this._node).ImageFile;

        public static async Task<Api.IChaserQuery<Rect2D>> CreateAsync(XmlReader reader, double minProbThreshold = 0.0)
        {
            var xmlRoot = await XmlTileNode.ReadXmlAsync(reader);
            // ALAN TODO refactor, separate the two concepts here: (1) packing to initial coordinates from probabilities,
            // (2) movement of coordinates...
            var root = new XmlChaserQueryNode2D(null, xmlRoot, new Rect2D(0.0, 1.0, 0.0, 1.0));
            var engine = new QueryEngine2D(root);
            return engine;
        }

        private XmlChaserQueryNode2D(IChaserQueryNode<Rect2D> parent, XmlTileNode node, Rect2D coords) : base(parent, node, coords)
        {
        }

        protected override async Task<IEnumerable<ChaserQueryNode<Rect2D>>> CalcChildrenAsync()
        {
            IReadOnlyList<XmlTileNode> children = this._node.Cast<XmlTileNode>().ToList();
            return children.Select(tile => new XmlChaserQueryNode2D(this, tile, tile.prespecifiedCoords)).ToList();
        }
    }
}
