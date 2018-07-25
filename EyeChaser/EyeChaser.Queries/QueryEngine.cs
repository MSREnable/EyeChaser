using EyeChaser.Api;
using System;
using System.Diagnostics;

namespace EyeChaser.Queries
{
    using Range1D = Tuple<double, double>;
    public abstract class QueryEngine : IChaserQuery<Range1D>
    {
        ChaserQueryNodeOffset<Range1D> _lowerBound;

        ChaserQueryNodeOffset<Range1D> _upperBound;

        public IChaserQueryNode<Range1D> Root { get; private set; }

        public double MinimumQueryProbability { get; set; } = 0.05;

        public double MinimumCumulatativeProbabilityTotal { get; set; } = 0.95;

        internal QueryEngine()
        {
        }

        internal void SetRoot(IChaserQueryNode<Range1D> root)
        {
            Root = root;
            _lowerBound = new ChaserQueryNodeOffset<Range1D>(Root, new Range1D(0, 0));
            _upperBound = new ChaserQueryNodeOffset<Range1D>(Root, new Range1D(1, 1));
        }

        public ChaserQueryNodeOffset<Range1D> MapToChild(ChaserQueryNodeOffset<Range1D> parent)
        {
            var child = parent;

            var parentOffset = parent.Offset;

            if (!(0 <= parentOffset.Item1 && parentOffset.Item2 <= 1))
            {
                throw new NotImplementedException("Need to walk up and down tree to do this!");
            }

            if (parentOffset.Item1 != parentOffset.Item2)
            {
                throw new NotImplementedException("Don't know how to work for spans");
            }

            var parentNode = parent.Node;

            if (!parentNode.IsUpdateNeeded)
            {
                using (var enumerator = parentNode.Children.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        var candidate = enumerator.Current;
                        while (candidate.QueryCoords.Item1 > parentOffset.Item1 && enumerator.MoveNext())
                        {
                            candidate = enumerator.Current;
                        }

                        child = new ChaserQueryNodeOffset<Range1D>(candidate,
                            new Range1D((parentOffset.Item1 - candidate.QueryCoords.Item1) / (candidate.QueryCoords.Item2 - candidate.QueryCoords.Item1),
                            (parentOffset.Item1 - candidate.QueryCoords.Item2) / (candidate.QueryCoords.Item2 - candidate.QueryCoords.Item1)));
                    }
                }
            }

            return child;
        }

        public ChaserQueryNodeOffset<Range1D> MapToParent(ChaserQueryNodeOffset<Range1D> child)
        {
            var childNode = child.Node;

            var parentNode = childNode.Parent;
            var parentOffset = new Range1D(childNode.QueryCoords.Item1 + child.Offset.Item1 * (childNode.QueryCoords.Item2 - childNode.QueryCoords.Item1),
                childNode.QueryCoords.Item1 + child.Offset.Item2 * (childNode.QueryCoords.Item2 - childNode.QueryCoords.Item1));

            var parent = new ChaserQueryNodeOffset<Range1D>(parentNode, parentOffset);

            return parent;
        }

        public void RemoveRoot()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(IChaserQueryNode<Range1D> node, Range1D coords)
        {
            Debug.Assert(coords.Item1 == coords.Item2);

            Debug.WriteLine($"Touched {node.Caption} at {coords}");

            var walker = new ChaserQueryNodeOffset<Range1D>(node, coords);
            while (walker.Node.Parent != null)
            {
                walker = MapToParent(walker);
                Debug.WriteLine($"  which is {walker.Node.Caption} at {walker.Offset}");
            }

            var oldRootCoords = walker.Node.QueryCoords;

            var maxMoveAmount = 0.05 * (oldRootCoords.Item2 - oldRootCoords.Item1);
            var expandFactor = 1.2;

            var navigateCenter = walker.Offset.Item1 * (oldRootCoords.Item2 - oldRootCoords.Item1);

            var moveAmount = Math.Min(maxMoveAmount, Math.Max(-maxMoveAmount, 0.5 - oldRootCoords.Item1 - navigateCenter));

            var movedRootCoords = new Range1D(oldRootCoords.Item1 + moveAmount, oldRootCoords.Item2 + moveAmount);

            var movedNavigationCenter = navigateCenter + moveAmount;

            var expandedRootCoords = new Range1D(movedNavigationCenter - expandFactor * (movedNavigationCenter - movedRootCoords.Item1),
                movedNavigationCenter + expandFactor * (movedRootCoords.Item2 - movedNavigationCenter));

            Debug.WriteLine($"Clicked at {navigateCenter} within {oldRootCoords.Item1}..{oldRootCoords.Item2}");
            Debug.WriteLine($"  moved by {moveAmount} to {movedRootCoords.Item1}..{movedRootCoords.Item2}");
            Debug.WriteLine($"  expanded to {expandedRootCoords.Item1}..{expandedRootCoords.Item2}");

            walker.Node.SetUpdate(expandedRootCoords);
        }
    }
}
