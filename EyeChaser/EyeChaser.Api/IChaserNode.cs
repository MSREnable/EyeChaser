using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EyeChaser.Api
{
    /// <summary>
    /// Untyped query result tree node.
    /// </summary>
    public interface IChaserNode : INotifyPropertyChanged, IEnumerable, INotifyCollectionChanged
    {
        /// <summary>
        /// Display caption for node.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Key, unique among peers, that can be used to order node.
        /// </summary>
        string SortKey { get; }

        /// <summary>
        /// Probability among peers of node.
        /// </summary>
        double Probability { get; }

        /// <summary>
        /// Load or refresh children.
        /// </summary>
        /// <returns></returns>
        Task RefreshChildrenAsync();
    }

    /// <summary>
    /// Type safe version of IChaserNode.
    /// </summary>
    /// <typeparam name="T">The type implementing this interface.</typeparam>
    public interface IChaserNode<T> : IChaserNode, IEnumerable<T>
        where T : class, IChaserNode<T>
    {
    }
}
