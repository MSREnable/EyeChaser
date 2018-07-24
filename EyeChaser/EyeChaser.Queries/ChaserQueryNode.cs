using EyeChaser.Api;
using EyeChaser.StaticModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EyeChaser.Queries
{
    internal class ChaserQueryNode : IChaserQueryNode
    {
        readonly IChaserNode _node;

        List<ChaserQueryNode> _list;

        public ChaserQueryNode(IChaserNode node)
        {
            _node = node;
            IsUpdateNeeded = true;
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

        public double QueryProbability { get; internal set; }

        public double QueryCommulativeProbability { get; internal set; }

        public IReadOnlyList<IChaserQueryNode> QueryChildren { get; private set; }

        public bool IsUpdateNeeded { get; private set; }

        public Task UpdateAsync()
        {
            var list = new List<ChaserQueryNode>();

            var dataProbabilitySum = 0.0;
            foreach (IChaserNode node in _node)
            {
                dataProbabilitySum += node.Probability;
            }

            var sum = 0.0;
            foreach (IChaserNode node in _node)
            {
                var queryProbability = node.Probability / dataProbabilitySum;

                var childNode = new ChaserQueryNode(node)
                {
                    QueryProbability = queryProbability,
                    QueryCommulativeProbability = sum
                };
                list.Add(childNode);

                sum += queryProbability;
            }

            if (list.Count == 0)
            {
                var blahNode = new XmlChaserNode
                {
                    Caption = "blah",
                    Probability = 0.5,
                };
                var childNode = new ChaserQueryNode(blahNode)
                {
                    QueryProbability = 0.9,
                    QueryCommulativeProbability = 0.05
                };
                list.Add(childNode);
            }

            QueryChildren = list;

            return Task.FromResult(true);
        }
    }
}