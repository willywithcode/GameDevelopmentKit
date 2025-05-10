namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    using System.Collections.Generic;

    public class Node
    {
        public enum Status
        {
            Success,
            Failure,
            Running,
        }

        public readonly string name;
        public readonly int    priority;

        public readonly List<Node> children = new();
        protected       int        currentChild;

        public Node(string name = "Node", int priority = 0)
        {
            this.name     = name;
            this.priority = priority;
        }

        public void AddChild(Node child)
        {
            this.children.Add(child);
        }

        public virtual Status Process()
        {
            return this.children[this.currentChild].Process();
        }

        public virtual void Reset()
        {
            this.currentChild = 0;
            foreach (var child in this.children) child.Reset();
        }
    }
}