using EyeChaser.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EyeChaser.Transforms
{
    public class AlphabeticChaserNode : IChaserNode, IComparable<AlphabeticChaserNode>
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

        public double Probability => _wrapped.Probability;

        public int CompareTo(AlphabeticChaserNode other)
        {
            return Caption.CompareTo(other.Caption);
        }

        public async Task<IEnumerable<IChaserNode>> GetChildrenAsync(double precision)
        {
            if (_sortedSet == null)
            {
                _sortedSet = new SortedSet<AlphabeticChaserNode>();

                var probabilitySum = 0.0;
                var maxProbability = 1.0;

                var enumerable = await _wrapped.GetChildrenAsync(precision);
                var enumerator = enumerable.GetEnumerator();
                while (_probabilityLimit <= maxProbability && enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    _sortedSet.Add(new AlphabeticChaserNode(current, _probabilityLimit));
                    maxProbability = current.Probability;
                    probabilitySum += current.Probability;
                }
            }

            return _sortedSet;
        }
    }
}
