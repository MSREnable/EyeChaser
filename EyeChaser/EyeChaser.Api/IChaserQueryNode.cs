using System.Collections.Generic;
using System.Threading.Tasks;

namespace EyeChaser.Api
{
    /// <summary>
    /// Tree from chaser query node.
    /// </summary>
    public interface IChaserQueryNode
    {
        /// <summary>
        /// The generation number of the node, being parent's generation plus one.
        /// </summary>
        int Generation { get; }

        /// <summary>
        /// The parent query node.
        /// </summary>
        IChaserQueryNode Parent { get; }

        /// <summary>
        /// The display caption for the node.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Unique key that can be used to sort nodes into a display order.
        /// </summary>
        string SortKey { get; }

        /// <summary>
        /// Probability of node among all peers.
        /// </summary>
        double PeerProbability { get; }

        /// <summary>
        /// Commulative peer probabilities withing sorted order of peers.
        /// </summary>
        double PeerCommulativeProbability { get; }

        /// <summary>
        /// Collection of all the node's children.
        /// </summary>
        IReadOnlyList<IChaserQueryNode> Children { get; }

        /// <summary>
        /// The probability of node within the query results.
        /// </summary>
        double QueryProbability { get; }

        /// <summary>
        /// The commulative probability sum of the preceding query probabilities, which may be negative.
        /// </summary>
        double QueryCommulativeProbability { get; }

        /// <summary>
        /// Access to the children that are part of the query.
        /// </summary>
        IReadOnlyList<IChaserQueryNode> QueryChildren { get; }

        /// <summary>
        /// Is the node's children ready for iteration.
        /// </summary>
        bool IsUpdateNeeded { get; }

        /// <summary>
        /// Update the children of the node.
        /// </summary>
        /// <returns></returns>
        Task UpdateAsync();
    }
}
