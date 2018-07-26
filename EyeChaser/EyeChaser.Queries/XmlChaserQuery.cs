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
            var root = new ChaserQueryNode<Range1D>(null, sortedRoot, Renormalize, new Range1D(0.0, 1.0));
            engine.SetRoot(root);
            return engine;
        }

        public static IReadOnlyList<Range1D> Renormalize(IReadOnlyList<Api.IChaserNode> nodes)
        {
            Range1D[] res = new Range1D[nodes.Count];
            double total = nodes.Select(ch => ch.Probability).Sum();
            double cumu = 0.0;
            for (int i = 0; i < nodes.Count; i++)
            {
                double next = (i == nodes.Count - 1) ? 1.0 : cumu + (nodes[i].Probability / total);
                res[i] = new Range1D(cumu, next);
                cumu = next;
            }
            return res;
        }
    }
}
