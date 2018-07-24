using EyeChaser.Api;
using System;
using System.Diagnostics;

namespace EyeChaser.Queries
{
    using Range1D = Tuple<double, double>;
    public abstract class QueryEngine : IChaserQuery<Range1D>
    {
        ChaserQueryNodeOffset _lowerBound;

        ChaserQueryNodeOffset _upperBound;

        public IChaserQueryNode<Range1D> Root { get; private set; }

        public double MinimumQueryProbability { get; set; } = 0.05;

        public double MinimumCumulatativeProbabilityTotal { get; set; } = 0.95;

        internal QueryEngine()
        {
        }

        internal void SetRoot(IChaserQueryNode<Range1D> root)
        {
            Root = root;
            _lowerBound = new ChaserQueryNodeOffset(Root, 0);
            _upperBound = new ChaserQueryNodeOffset(Root, 1);
        }

        public ChaserQueryNodeOffset MapToChild(ChaserQueryNodeOffset parent)
        {
            var parentOffset = parent.Offset;

            if (!(0 <= parentOffset && parentOffset <= 1))
            {
                throw new NotImplementedException("Need to walk up and down tree to do this!");
            }

            var parentNode = parent.Node;

            if (!parentNode.IsUpdateNeeded)
            {
                using (var enumerator = parentNode.Children.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        var candidate = enumerator.Current;
                        while (candidate.QueryCoords.Item1 > parentOffset && enumerator.MoveNext())
                        {
                            candidate = enumerator.Current;
                        }

                        return new ChaserQueryNodeOffset(candidate, (parentOffset - candidate.QueryCoords.Item1) / (candidate.QueryCoords.Item2 - candidate.QueryCoords.Item1));
                    }
                }
            }

            return parent;
        }

        public ChaserQueryNodeOffset MapToParent(ChaserQueryNodeOffset child)
        {
            var childNode = child.Node;

            var parentNode = childNode.Parent;
            var parentOffset = childNode.QueryCoords.Item1 + (childNode.QueryCoords.Item2 - childNode.QueryCoords.Item1) * child.Offset;

            var parent = new ChaserQueryNodeOffset(parentNode, parentOffset);

            return parent;
        }

        public void RemoveRoot()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(IChaserQueryNode<Range1D> node, Range1D coords)
        {
            Debug.WriteLine($"Touched {node.Caption} at {coords}");
        }
    }
}
