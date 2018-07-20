using EyeChaser.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EyeChaser.Transforms
{
    public class AlphabeticChaserNode : IChaserNode<AlphabeticChaserNode>, IComparable<AlphabeticChaserNode>
    {
        readonly IChaserNode _wrapped;

        readonly double _probabilityLimit;

        SortedSet<AlphabeticChaserNode> _sortedSet;

        public AlphabeticChaserNode(IChaserNode wrapped, double probabilityLimit)
        {
            _wrapped = wrapped;
            _probabilityLimit = probabilityLimit;
        }

        public string Caption => _wrapped.Caption;

        public string SortKey => _wrapped.SortKey;

        public double Probability => _wrapped.Probability;

        public bool IsChildrenPopulated => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }
        public event NotifyCollectionChangedEventHandler CollectionChanged { add { } remove { } }

        public int CompareTo(AlphabeticChaserNode other)
        {
            return SortKey.CompareTo(other.SortKey);
        }

        public IEnumerator<AlphabeticChaserNode> GetEnumerator()
        {
            if (_sortedSet == null)
            {
                _sortedSet = new SortedSet<AlphabeticChaserNode>();

                var probabilitySum = 0.0;
                var maxProbability = 1.0;

                var enumerator = _wrapped.GetEnumerator();
                while (_probabilityLimit <= maxProbability && enumerator.MoveNext())
                {
                    var current = (IChaserNode)enumerator.Current;

                    _sortedSet.Add(new AlphabeticChaserNode(current, _probabilityLimit));
                    maxProbability = current.Probability;
                    probabilitySum += current.Probability;
                }
            }

            return _sortedSet.GetEnumerator();
        }

        public Task RefreshChildrenAsync()
        {
            return Task.FromResult(false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
