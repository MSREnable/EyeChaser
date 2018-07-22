using EyeChaser.Api;
using System;

namespace EyeChaser.Queries
{
    public abstract class QueryEngine : IChaserQuery
    {
        ChaserQueryNodeOffset _lowerBound;

        ChaserQueryNodeOffset _upperBound;

        public IChaserQueryNode Root { get; private set; }

        public double MinimumQueryProbability { get; set; } = 0.05;

        public double MinimumCumulatativeProbabilityTotal { get; set; } = 0.95;

        internal QueryEngine(ChaserQueryNode root)
        {
            Root = root;
            root.QueryCommulativeProbability = 0;
            root.QueryProbability = 1;
            _lowerBound = new ChaserQueryNodeOffset(Root, 0);
            _upperBound = new ChaserQueryNodeOffset(Root, 1);
        }

        public ChaserQueryNodeOffset MapToChild(ChaserQueryNodeOffset parent)
        {
            var child = parent;

            var parentOffset = parent.Offset;

            if (!(0 <= parentOffset && parentOffset <= 1))
            {
                throw new NotImplementedException("Need to walk up and down tree to do this!");
            }

            var parentNode = parent.Node;

            if (!parentNode.IsUpdateNeeded)
            {
                using (var enumerator = parentNode.QueryChildren.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        var candidate = enumerator.Current;
                        while (candidate.QueryCommulativeProbability + candidate.QueryProbability < parentOffset && enumerator.MoveNext())
                        {
                            candidate = enumerator.Current;
                        }

                        child = new ChaserQueryNodeOffset(candidate, (parentOffset - candidate.QueryCommulativeProbability) / candidate.QueryProbability);
                    }
                }
            }

            return child;
        }

        public ChaserQueryNodeOffset MapToParent(ChaserQueryNodeOffset child)
        {
            var childNode = child.Node;

            var parentNode = childNode.Parent;
            var parentOffset = childNode.QueryCommulativeProbability + childNode.QueryProbability * childNode.QueryProbability;

            var parent = new ChaserQueryNodeOffset(parentNode, parentOffset);

            return parent;
        }

        public void RemoveRoot()
        {
            throw new NotImplementedException();
        }

        public void Requery(IChaserQueryNode middle, double offset, double bounding)
        {
            throw new NotImplementedException();
        }

        public void Requery(IChaserQueryNode lowerLimit, double lowerOffset, IChaserQueryNode upperLimit, double upperOffset)
        {
            throw new NotImplementedException();
        }
    }
}
