using System;

namespace EyeChaser.Api
{
    using Range1D = Tuple<double, double>;
    /// <summary>
    /// Structure containing of a query node and the offset position within it.
    /// </summary>
    public struct ChaserQueryNodeOffset
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">The query node.</param>
        /// <param name="offset">The offset within the query node.</param>
        public ChaserQueryNodeOffset(IChaserQueryNode<Range1D> node, double offset)
        {
            Node = node;
            Offset = offset;
        }

        /// <summary>
        /// The node.
        /// </summary>
        public IChaserQueryNode<Range1D> Node { get; }

        /// <summary>
        /// The offset within the node. Normally in the range 0..1.
        /// </summary>
        public double Offset { get; }
    }
}