using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

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
        /// Probability among peers of node.
        /// </summary>
        double Probability { get; }
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
