using EyeChaser.Api;
using EyeChaser.StaticModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace EyeChaser.Queries
{
    // This is now a fairly plan DTO, could maybe fold into IChasterQueryNode?
    public abstract class ChaserQueryNode<Coords> : IChaserQueryNode<Coords>, INotifyPropertyChanged
    {
        protected readonly IChaserNode _node;
        Coords _coords;

        List<ChaserQueryNode<Coords>> _list = new List<ChaserQueryNode<Coords>>();

        public ChaserQueryNode(IChaserQueryNode<Coords> parent, IChaserNode node, Coords coords)
        {
            Parent = parent;
            _node = node;
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

        public string SortKey => _node.SortKey;

        public Coords QueryCoords => _coords;

        public IReadOnlyList<IChaserQueryNode<Coords>> Children { get => _list; }

        public void SetUpdate(Coords coords)
        {
            IsUpdateNeeded = true;
            _coords = coords;

            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
        }

        public bool IsUpdateNeeded { get; private set; }

        public virtual async Task UpdateAsync()
        {
            IEnumerable<ChaserQueryNode<Coords>> newChildren = await CalcChildrenAsync();
            _list.Clear();
            _list.AddRange(newChildren);
        }

        protected abstract Task<IEnumerable<ChaserQueryNode<Coords>>> CalcChildrenAsync();
    }
}