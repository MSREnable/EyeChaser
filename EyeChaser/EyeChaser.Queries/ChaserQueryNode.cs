using EyeChaser.Api;
using EyeChaser.StaticModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace EyeChaser.Queries
{
    // This is now a fairly plan DTO (except UpdateAsync - could move that into the packing algorithm class?), could maybe fold into IChasterQueryNode?
    internal class ChaserQueryNode<Coords> : IChaserQueryNode<Coords>
    {
        readonly IChaserNode _node;
        Func<IReadOnlyList<IChaserNode>, IReadOnlyList<Coords>> _packingAlgorithm;
        Coords _coords;
        
        List<ChaserQueryNode<Coords>> _list = new List<ChaserQueryNode<Coords>>();

        public ChaserQueryNode(IChaserNode node, Func<IReadOnlyList<IChaserNode>, IReadOnlyList<Coords>> packingAlgorithm, Coords coords)
        {
            _node = node;
            _packingAlgorithm = packingAlgorithm;
            _coords = coords;
        }

        public int Generation { get; internal set; }

        public IChaserQueryNode<Coords> Parent { get; internal set; }

        public string Caption => _node.Caption;

        public string SortKey => _node.SortKey;

        public Coords QueryCoords => _coords;

        public IReadOnlyList<IChaserQueryNode<Coords>> Children { get => _list; }
        
        public IReadOnlyList<IChaserQueryNode<Coords>> QueryChildren => throw new System.NotImplementedException();

        public bool IsUpdateNeeded => throw new System.NotImplementedException();

        public async Task UpdateAsync()
        {
            await Task.Run(() =>
            {
                _list.Clear();
                IReadOnlyList<IChaserNode> children = _node.Cast<IChaserNode>().ToList();
                /*if (children.Count == 0)
                {
                    children = new IChaserNode[] { new XmlChaserNode {
                        Caption = "blah",
                        Probability = 1
                    }};
                }*/
                _list.AddRange(children.Zip(_packingAlgorithm(children), (child, coords) => new ChaserQueryNode<Coords>(child, _packingAlgorithm, coords)));

                if (_list.Count == 0)
                {
                    // Here we copy coords from parent, which might not be appropriate
                    // Other options: don't populate (no children) - signals to UI, don't allow further movement within this node.
                    // Add method to packing algorithm to get default coords.
                    // Pass single child to packing algorithm (as in comment above).
                    var blahNode = new XmlChaserNode
                    {
                        Caption = "blah",
                        Probability = 1
                    };

                    _list.Add(new ChaserQueryNode<Coords>(blahNode, _packingAlgorithm, _coords));
                }
            });

        }
    }
}