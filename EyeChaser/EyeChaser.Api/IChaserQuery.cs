namespace EyeChaser.Api
{
    /// <summary>
    /// Interface defining chaser query mechanism.
    /// </summary>
    public interface IChaserQuery<Coords>
    {
        /// <summary>
        /// The root of the query tree.
        /// </summary>
        IChaserQueryNode<Coords> Root { get; }

        /// <summary>
        /// The minimum probability of a node to be included in the tree.
        /// </summary>
        double MinimumQueryProbability { get; set; }

        /// <summary>
        /// The minimum total cumulative probability of nodes added to tree.
        /// </summary>
        double MinimumCumulativeProbabilityTotal { get; set; }

        /// <summary>
        /// Remove the query tree root.
        /// </summary>
        void RemoveRoot();

        /// <summary>
        /// Requery to the specificied node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="coords"></param>
        void NavigateTo(IChaserQueryNode<Coords> node, Coords coords);

        /// <summary>
        /// Convert a reference to a child node to one in its parent.
        /// </summary>
        /// <param name="child">The pair representing the child node position.</param>
        /// <returns>The corresponding node position in the parent.</returns>
        ChaserQueryNodeOffset<Coords> MapToParent(ChaserQueryNodeOffset<Coords> child);

        /// <summary>
        /// Convert a reference to a parent node to one of its children.
        /// </summary>
        /// <param name="parent">The pair representing the parent node position.</param>
        /// <returns>THe corresponding node position in a child of the parent.</returns>
        ChaserQueryNodeOffset<Coords> MapToChild(ChaserQueryNodeOffset<Coords> parent);
    }
}
