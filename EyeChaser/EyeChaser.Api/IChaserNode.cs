using System.Collections.Generic;
using System.Threading.Tasks;

namespace EyeChaser.Api
{
    /// <summary>
    /// Untyped query result tree node.
    /// </summary>
    public interface IChaserNode
    {
        /// <summary>
        /// Display caption for node.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Probability among peers of node.
        /// </summary>
        double Probability { get; }

        /// <summary>
        /// Get children of node.
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        Task<IEnumerable<IChaserNode>> GetChildrenAsync(double precision);
    }
}
