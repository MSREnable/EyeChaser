namespace EyeChaser.Api
{
    /// <summary>
    /// Interface defining chaser query mechanism.
    /// </summary>
    public interface IChaserQuery
    {
        /// <summary>
        /// The root of the query tree.
        /// </summary>
        IChaserQueryNode Root { get; }

        /// <summary>
        /// The minimum probability of a node to be included in the tree.
        /// </summary>
        double MinimumQueryProbability { get; set; }

        /// <summary>
        /// The minimum total cumulative probability of nodes added to tree.
        /// </summary>
        double MinimumCumulatativeProbabilityTotal { get; set; }

        /// <summary>
        /// Remove the query tree root.
        /// </summary>
        void RemoveRoot();

        /// <summary>
        /// Rejig the tree to reflect a new query,
        /// </summary>
        /// <param name="middle">The node at the center point of the query.</param>
        /// <param name="offset">The offset within the node of the center point.</param>
        /// <param name="bounding">THe offset plus and minus from the center point to include in the query results.</param>
        void Requery(IChaserQueryNode middle, double offset, double bounding);

        /// <summary>
        /// Rejig the tree to reflect a new query.
        /// </summary>
        /// <param name="lowerLimit">The node at the lower limit of the query.</param>
        /// <param name="lowerOffset">The offset within the lower limit to represent the lower limit of the tree.</param>
        /// <param name="upperLimit">The node at the upper limit of the query.</param>
        /// <param name="upperOffset">The offset within the upper limit to represent the uppwer limit of the tree.</param>
        void Requery(IChaserQueryNode lowerLimit, double lowerOffset, IChaserQueryNode upperLimit, double upperOffset);

        /// <summary>
        /// Convert a reference to a child node to one in its parent.
        /// </summary>
        /// <param name="child">The pair representing the child node position.</param>
        /// <returns>The corresponding node position in the parent.</returns>
        ChaserQueryNodeOffset MapToParent(ChaserQueryNodeOffset child);

        /// <summary>
        /// Convert a reference to a parent node to one of its children.
        /// </summary>
        /// <param name="parent">The pair representing the parent node position.</param>
        /// <returns>THe corresponding node position in a child of the parent.</returns>
        ChaserQueryNodeOffset MapToChild(ChaserQueryNodeOffset parent);
    }
}
