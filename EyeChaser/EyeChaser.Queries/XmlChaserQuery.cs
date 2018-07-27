using EyeChaser.Api;
using EyeChaser.StaticModel;
using EyeChaser.Transforms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.Queries
{
    public class XmlChaserQueryEngine
    {
        private XmlChaserQueryEngine()
        {
        }

        public static async Task<Api.IChaserQuery<Range1D>> CreateAsync(XmlReader reader, double minProbThreshold = 0.0)
        {
            var xmlRoot = await XmlChaserNode.ReadXmlAsync(reader);
            var sortedRoot = new AlphabeticChaserNode(xmlRoot, minProbThreshold);
            // ALAN TODO refactor, separate the two concepts here: (1) packing to initial coordinates from probabilities,
            // (2) movement of coordinates...
            var engine = new QueryEngine();
            var root = new RenormalizingNode(null, sortedRoot, new Range1D(0.0, 1.0));
            engine.SetRoot(root);
            return engine;
        }

        private class RenormalizingNode : ChaserQueryNode<Range1D>
        {
            public RenormalizingNode(IChaserQueryNode<Range1D> parent, IChaserNode node, Range1D coords) : base(parent, node, coords)
            {
            }

            protected override async Task<IEnumerable<ChaserQueryNode<Range1D>>> CalcChildrenAsync()
            {
                IReadOnlyList<IChaserNode> children = this._node.Cast<IChaserNode>().ToList();
                if (children.Count == 0)
                {
                    children = new[]
                    {
                    new XmlChaserNode { Caption = "aardvark", Probability = 0.1 },
                    new XmlChaserNode { Caption = "blah", Probability = 0.8 },
                    new XmlChaserNode { Caption = "wibble", Probability = 0.1 }
                    };
                }
                ChaserQueryNode<Range1D>[] res = new ChaserQueryNode<Range1D>[children.Count];
                double total = children.Select(ch => ch.Probability).Sum();
                double cumu = 0.0;
                for (int i = 0; i < children.Count; i++)
                {
                    double next = (i == children.Count - 1) ? 1.0 : cumu + (children[i].Probability / total);
                    res[i] = new RenormalizingNode(this, children[i], new Range1D(cumu, next));
                    cumu = next;
                }
                return res;
            }

        }
    }
}
