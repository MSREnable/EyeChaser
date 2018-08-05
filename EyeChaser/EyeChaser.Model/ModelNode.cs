using EyeChaser.Api;
using EyeChaser.Queries;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EyeChaser.Model
{
    public class ModelNode
    {
        readonly IChaserQueryNode<Range1D> _root;

        ObservableCollection<ModelNode> _children = new ObservableCollection<ModelNode>();

        internal ModelNode(IChaserQueryNode<Range1D> node)
        {
            _root = node;
            Children = new ReadOnlyObservableCollection<ModelNode>(_children);
        }

        public ReadOnlyObservableCollection<ModelNode> Children { get; }

        public string Caption => _root.Caption;

        public Range1D VisibleRange { get; internal set; }

        public double BoundSize => _root.QueryCoords.BoundSize;

        internal async Task UpdateAsync(double limit)
        {
            var parent = _root;

            if (parent.IsUpdateNeeded)
            {
                await parent.UpdateAsync(limit);
            }

            var childControlIndex = 0;
            var currentChildControl = childControlIndex < _children.Count ? _children[childControlIndex] : null;

            var visibleRange = VisibleRange;

            foreach (IChaserQueryNode<Range1D> child in parent.Children)
            {
                var onScreenProb = Math.Min(1.0, child.QueryCoords.UpperBound) - Math.Max(0.0, child.QueryCoords.LowerBound);
                if (onScreenProb >= limit && visibleRange.LowerBound <= child.QueryCoords.UpperBound && child.QueryCoords.LowerBound <= visibleRange.UpperBound)
                {
                    while (currentChildControl != null && string.CompareOrdinal(currentChildControl.Caption, child.Caption) < 0)
                    {
                        _children.RemoveAt(childControlIndex);
                        currentChildControl = childControlIndex < _children.Count ? _children[childControlIndex] : null;
                    }

                    ModelNode control;

                    double overallProb = child.QueryCoords.BoundSize;

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

                    control.VisibleRange = new Range1D(Math.Max(0, (visibleRange.LowerBound - child.QueryCoords.LowerBound) / overallProb),
                        Math.Min(1, 1 - (child.QueryCoords.UpperBound - visibleRange.UpperBound) / overallProb));
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
