using EyeChaser.Api;
using EyeChaser.Queries;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EyeChaser.Model
{
    public class RootModel
    {
        IChaserQueryNode<Range1D> _root;

        readonly ObservableCollection<ModelNode> _children = new ObservableCollection<ModelNode>();

        public RootModel(IChaserQueryNode<Range1D> root)
        {
            _root = root;
            Children = new ReadOnlyObservableCollection<ModelNode>(_children);
        }

        public ReadOnlyObservableCollection<ModelNode> Children { get; }

        public async Task UpdateAsync(double limit)
        {
            var parent = _root;

            var parentSpan = parent.QueryCoords;
            var parentSize = parentSpan.BoundSize;

            if (parent.IsUpdateNeeded)
            {
                await parent.UpdateAsync(limit);
            }

            var childControlIndex = 0;
            var currentChildControl = childControlIndex < _children.Count ? _children[childControlIndex] : null;

            foreach (IChaserQueryNode<Range1D> child in parent.Children)
            {
                double onScreenProb = parentSize * Math.Min(1.0, child.QueryCoords.UpperBound) - Math.Max(0.0, child.QueryCoords.LowerBound);
                var startPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.LowerBound;
                var endPosition = parentSpan.LowerBound + parentSize * child.QueryCoords.UpperBound;
                if (onScreenProb >= limit && 0 <= endPosition && startPosition <= 1)
                {
                    while (currentChildControl != null && string.CompareOrdinal(currentChildControl.Caption, child.Caption) < 0)
                    {
                        _children.RemoveAt(childControlIndex);
                        currentChildControl = childControlIndex < _children.Count ? _children[childControlIndex] : null;
                    }

                    ModelNode control;

                    var overallProb = endPosition - startPosition;

                    if (currentChildControl == null || string.CompareOrdinal(currentChildControl.Caption, child.Caption) != 0)
                    {
                        control = new ModelNode(child);

                        _children.Insert(childControlIndex, control);

                        childControlIndex++;
                    }
                    else
                    {
                        control = currentChildControl;

                        childControlIndex++;
                        currentChildControl = childControlIndex < _children.Count ? _children[childControlIndex] : null;
                    }

                    control.VisibleRange = new Range1D(Math.Max(0, -startPosition / overallProb), Math.Min(1, (1 - startPosition) / overallProb));
                }
            }

            while (childControlIndex < _children.Count)
            {
                _children.RemoveAt(_children.Count - 1);
            }

            foreach (var node in _children)
            {
                await node.UpdateAsync(limit / node.BoundSize);
            }
        }
    }
}
