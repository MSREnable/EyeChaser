using EyeChaser.Api;
using System;
using System.Diagnostics;

namespace EyeChaser.Queries
{
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

            if (!(0 <= parentOffset.LowerBound && parentOffset.UpperBound <= 1))
            {
                throw new NotImplementedException("Need to walk up and down tree to do this!");
            }

            if (parentOffset.LowerBound != parentOffset.UpperBound)
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
                        while (candidate.QueryCoords.LowerBound > parentOffset.LowerBound && enumerator.MoveNext())
                        {
                            candidate = enumerator.Current;
                        }

                        child = new ChaserQueryNodeOffset<Range1D>(candidate,
                            new Range1D((parentOffset.LowerBound - candidate.QueryCoords.LowerBound) / (candidate.QueryCoords.BoundSize),
                            (parentOffset.LowerBound - candidate.QueryCoords.UpperBound) / (candidate.QueryCoords.BoundSize)));
                    }
                }
            }

            return child;
        }

        public ChaserQueryNodeOffset<Range1D> MapToParent(ChaserQueryNodeOffset<Range1D> child)
        {
            var childNode = child.Node;

            var parentNode = childNode.Parent;
            var parentOffset = new Range1D(childNode.QueryCoords.LowerBound + child.Offset.LowerBound * (childNode.QueryCoords.BoundSize),
                childNode.QueryCoords.LowerBound + child.Offset.UpperBound * (childNode.QueryCoords.BoundSize));

            var parent = new ChaserQueryNodeOffset<Range1D>(parentNode, parentOffset);

            return parent;
        }

        public void RemoveRoot()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(IChaserQueryNode<Range1D> node, Range1D coords)
        {
            Debug.Assert(coords.LowerBound == coords.UpperBound);

            Debug.WriteLine($"Touched {node.Caption} at {coords}");

            var walker = new ChaserQueryNodeOffset<Range1D>(node, coords);
            while (walker.Node.Parent != null)
            {
                walker = MapToParent(walker);
                Debug.WriteLine($"  which is {walker.Node.Caption} at {walker.Offset}");
            }

            var oldRootCoords = walker.Node.QueryCoords;

            var maxMoveAmount = 0.05 * (oldRootCoords.UpperBound - oldRootCoords.LowerBound);
            var expandFactor = 1.2;

            var navigateCenter = walker.Offset.LowerBound * (oldRootCoords.UpperBound - oldRootCoords.LowerBound);

            var moveAmount = Math.Min(maxMoveAmount, Math.Max(-maxMoveAmount, 0.5 - oldRootCoords.LowerBound - navigateCenter));

            var movedRootCoords = new Range1D(oldRootCoords.LowerBound + moveAmount, oldRootCoords.UpperBound + moveAmount);

            var movedNavigationCenter = navigateCenter + moveAmount;

            var expandedRootCoords = new Range1D(movedNavigationCenter - expandFactor * (movedNavigationCenter - movedRootCoords.LowerBound),
                movedNavigationCenter + expandFactor * (movedRootCoords.UpperBound - movedNavigationCenter));

            Debug.WriteLine($"Clicked at {navigateCenter} within {oldRootCoords.LowerBound}..{oldRootCoords.UpperBound}");
            Debug.WriteLine($"  moved by {moveAmount} to {movedRootCoords.LowerBound}..{movedRootCoords.UpperBound}");
            Debug.WriteLine($"  expanded to {expandedRootCoords.LowerBound}..{expandedRootCoords.UpperBound}");

            walker.Node.SetUpdate(expandedRootCoords);
        }
    }
}
