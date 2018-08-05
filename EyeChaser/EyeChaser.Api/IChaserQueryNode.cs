using System.Collections.Generic;
using System.Threading.Tasks;

namespace EyeChaser.Api
{
    /// <summary>
    /// Tree from chaser query node.
    /// </summary>
    public interface IChaserQueryNode<Coords>
    {
        /// <summary>
        /// The generation number of the node, being parent's generation plus one.
        /// </summary>
        int Generation { get; }

        /// <summary>
        /// The parent query node.
        /// </summary>
        IChaserQueryNode<Coords> Parent { get; }

        /// <summary>
        /// The display caption for the node.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Collection of the node's children being part of the query
        /// </summary>
        IReadOnlyList<IChaserQueryNode<Coords>> Children { get; }

        /// <summary>
        /// The probability of node within the query results.
        /// </summary>
        Coords QueryCoords { get; }

        /// <summary>
        /// Set the node to update.
        /// </summary>
        /// <param name="coords"></param>
        void SetUpdate(Coords coords);

        /// <summary>
        /// Is the node's children ready for iteration.
        /// </summary>
        bool IsUpdateNeeded { get; }

        /// <summary>
        /// Update the children of the node.
        /// </summary>
        /// <returns></returns>
        Task UpdateAsync(double limit);
    }
}
