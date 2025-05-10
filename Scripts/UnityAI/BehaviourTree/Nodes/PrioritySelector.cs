namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    using System.Collections.Generic;
    using ZLinq;

    public class PrioritySelector : Selector
    {
        private List<Node> sortedChildren;
        private List<Node> SortedChildren => this.sortedChildren ??= this.SortChildren();

        protected virtual List<Node> SortChildren()
        {
            return this.children.AsValueEnumerable().OrderByDescending(child => child.priority).ToList();
        }

        public PrioritySelector(string name, int priority = 0) : base(name, priority) { }

        public override void Reset()
        {
            base.Reset();
            this.sortedChildren = null;
        }

        public override Status Process()
        {
            foreach (var child in this.SortedChildren)
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        this.Reset();
                        return Status.Success;
                    default:
                        continue;
                }

            this.Reset();
            return Status.Failure;
        }
    }
}