using System.Collections.Generic;
using System.Threading.Tasks;
using EyeChaser.Api;

namespace EyeChaser.StaticModel
{
    internal class ChaserQueryNode : IChaserQueryNode
    {
        readonly IChaserNode _node;

        List<ChaserQueryNode> _list;

        public ChaserQueryNode(IChaserNode node)
        {
            _node = node;
        }

        public int Generation { get; internal set; }

        public IChaserQueryNode Parent { get; internal set; }

        public string Caption => _node.Caption;

        public string SortKey => _node.SortKey;

        public double PeerProbability => _node.Probability;

        public double PeerCommulativeProbability { get; internal set; }

        public IReadOnlyList<IChaserQueryNode> Children
        {
            get
            {
                if (_list == null)
                {
                    _list = new List<ChaserQueryNode>();

                    foreach (IChaserNode node in _node)
                    {
                        var childNode = new ChaserQueryNode(node);
                        _list.Add(childNode);
                    }

                    if (_list.Count == 0)
                    {
                        var blahNode = new XmlChaserNode
                        {
                            Caption = "blah",
                            Probability = 1
                        };
                        var childNode = new ChaserQueryNode(blahNode);
                        _list.Add(childNode);
                    }
                }

                return _list;
            }
        }

        public double QueryProbability => throw new System.NotImplementedException();

        public double QueryCommulativeProbability => throw new System.NotImplementedException();

        public IReadOnlyList<IChaserQueryNode> QueryChildren => throw new System.NotImplementedException();

        public bool IsUpdateNeeded => throw new System.NotImplementedException();

        public Task UpdateAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}