using EyeChaser.StaticModel;
using EyeChaser.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.Queries
{
    using Range1D = Tuple<double, double>;
    public class XmlChaserQueryEngine : QueryEngine
    {
        XmlChaserQueryEngine()
        {
        }

        public static async Task<XmlChaserQueryEngine> CreateAsync(XmlReader reader, double minProbThreshold = 0.0)
        {
            var xmlRoot = await XmlChaserNode.ReadXmlAsync(reader);
            var sortedRoot = new AlphabeticChaserNode(xmlRoot, minProbThreshold);
            var engine = new XmlChaserQueryEngine();
            var root = new ChaserQueryNode<Range1D>(engine, null, sortedRoot, Renormalize, Tuple.Create(0.0, 1.0));
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
                res[i] = Tuple.Create(cumu, next);
                cumu = next;
            }
            return res;
        }
    }
}
