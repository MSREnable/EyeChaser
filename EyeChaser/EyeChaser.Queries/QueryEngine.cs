using EyeChaser.Api;
using System;
using System.Diagnostics;

namespace EyeChaser.Queries
{
    public class QueryEngine : IChaserQuery<Range1D>
    {
        public IChaserQueryNode<Range1D> Root { get; private set; }

        public double MinimumQueryProbability { get; set; } = 0.05;

        public double MinimumCumulativeProbabilityTotal { get; set; } = 0.95;

        internal QueryEngine()
        {
        }

        internal void SetRoot(IChaserQueryNode<Range1D> root)
        {
            Root = root;
        }

        public void RemoveRoot()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(Range1D coords)
        {
            Debug.Assert(coords.LowerBound == coords.UpperBound);

            Debug.WriteLine($"Touched at {coords}");

            var oldRootCoords = Root.QueryCoords;

            // First expand about the point that was clicked, so that point stays under the mouse
            var expandFactor = 1.2;

            var navigateCenter = coords.LowerBound;

            var expandedRootCoords = new Range1D((oldRootCoords.LowerBound - navigateCenter) * expandFactor + navigateCenter,
                (oldRootCoords.UpperBound - navigateCenter) * expandFactor + navigateCenter);

            // Now move a bit towards the middle...not clear we necessarily want this but maybe?
            var maxMoveAmount = 0.05; // Proportion of screen size 
            var moveAmount = Math.Min(maxMoveAmount, Math.Max(-maxMoveAmount, 0.5 - navigateCenter));

            var movedRootCoords = new Range1D(expandedRootCoords.LowerBound + moveAmount, expandedRootCoords.UpperBound + moveAmount);

            Debug.WriteLine($"Clicked at {navigateCenter} within {oldRootCoords.LowerBound}..{oldRootCoords.UpperBound}");
            Debug.WriteLine($"  expanded to {expandedRootCoords.LowerBound}..{expandedRootCoords.UpperBound}");
            Debug.WriteLine($"  moved by {moveAmount} to {movedRootCoords.LowerBound}..{movedRootCoords.UpperBound}");

            Root.SetUpdate(movedRootCoords);
        }
    }
}
