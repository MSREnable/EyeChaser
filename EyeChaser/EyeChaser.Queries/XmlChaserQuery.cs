using EyeChaser.Api;
using System.Collections.Generic;
using System.Linq;

namespace EyeChaser.Queries
{
    public static class XmlChaserQueryEngine
    {
        public static Api.IChaserQuery<Range1D> Create(IChaserNode root)
        {
            // ALAN TODO refactor, separate the two concepts here: (1) packing to initial coordinates from probabilities,
            // (2) movement of coordinates...
            var engine = new QueryEngine();
            var queryRoot = new ChaserQueryNode<Range1D>(null, root, Renormalize, new Range1D(0.0, 1.0));
            engine.SetRoot(queryRoot);
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
