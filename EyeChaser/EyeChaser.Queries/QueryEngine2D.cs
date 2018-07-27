using EyeChaser.Api;
using EyeChaser.StaticModel;
using System;
using System.Diagnostics;

namespace EyeChaser.Queries
{
    public class QueryEngine2D : IChaserQuery<Rect2D>
    {
        public IChaserQueryNode<Rect2D> Root { get; private set; }

        public double MinimumQueryProbability { get; set; } = 0.05;

        public double MinimumCumulativeProbabilityTotal { get; set; } = 0.95;

        internal QueryEngine2D()
        {
        }

        public void RemoveRoot()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(Rect2D coords)
        {
            Debug.Assert(coords.Left == coords.Right);
            Debug.Assert(coords.Top == coords.Bottom);

            Debug.WriteLine($"Touched at {coords}");

            var oldRootCoords = Root.QueryCoords;

            // First expand about the point that was clicked, so that point stays under the mouse
            var expandFactor = 1.2;

            var navigateX = coords.Left;
            var navigateY = coords.Top;

            var expandedRootCoords = new Rect2D((oldRootCoords.Left - navigateX) * expandFactor + navigateX,
                (oldRootCoords.Right - navigateX) * expandFactor + navigateX,
                (oldRootCoords.Top - navigateY) * expandFactor + navigateY,
                (oldRootCoords.Bottom - navigateY) * expandFactor + navigateY);

            // Now move a bit towards the middle...not clear we necessarily want this but maybe?
            var maxMoveAmount = 0.05; // Proportion of screen size 
            var moveX = Math.Min(maxMoveAmount, Math.Max(-maxMoveAmount, 0.5 - navigateX));
            var moveY = Math.Min(maxMoveAmount, Math.Max(-maxMoveAmount, 0.5 - navigateY));

            var movedRootCoords = new Rect2D(expandedRootCoords.Left + moveX, expandedRootCoords.Right + moveX,
                expandedRootCoords.Top + moveY, expandedRootCoords.Bottom + moveY);

            Root.SetUpdate(movedRootCoords);
        }
    }
}
