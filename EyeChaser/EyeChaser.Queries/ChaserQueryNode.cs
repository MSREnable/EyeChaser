using EyeChaser.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace EyeChaser.Queries
{
    // This is now a fairly plan DTO (except UpdateAsync - could move that into the packing algorithm class?), could maybe fold into IChasterQueryNode?
    internal class ChaserQueryNode<Coords> : IChaserQueryNode<Coords>, INotifyPropertyChanged
    {
        readonly IChaserNode _node;
        Func<IReadOnlyList<IChaserNode>, IReadOnlyList<Coords>> _packingAlgorithm;
        Coords _coords;

        List<ChaserQueryNode<Coords>> _list = new List<ChaserQueryNode<Coords>>();

        public ChaserQueryNode(IChaserQueryNode<Coords> parent, IChaserNode node, Func<IReadOnlyList<IChaserNode>, IReadOnlyList<Coords>> packingAlgorithm, Coords coords)
        {
            Parent = parent;
            _node = node;
            _packingAlgorithm = packingAlgorithm;
            _coords = coords;
            IsUpdateNeeded = true;
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged += value;
            }
        }
        PropertyChangedEventHandler _propertyChanged;

        public int Generation { get; internal set; }

        public IChaserQueryNode<Coords> Parent { get; internal set; }

        public string Caption => _node.Caption;

        public Coords QueryCoords => _coords;

        public IReadOnlyList<IChaserQueryNode<Coords>> Children { get => _list; }

        public void SetUpdate(Coords coords)
        {
            IsUpdateNeeded = true;
            _coords = coords;

            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
        }

        public bool IsUpdateNeeded { get; private set; }

        public async Task UpdateAsync()
        {
            _list.Clear();
            var children = new List<IChaserNode>(await _node.GetChildrenAsync(0));
            _list.AddRange(children.Zip(_packingAlgorithm(children), (child, coords) => new ChaserQueryNode<Coords>(this, child, _packingAlgorithm, coords)));
        }

        public override string ToString()
        {
            return Parent == null ? Caption : Parent.ToString() + ".." + Caption;
        }
    }
}