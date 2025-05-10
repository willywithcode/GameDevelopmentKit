namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    using System.Text;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Policies;
    using UnityEngine;

    public class BehaviourTree : Node
    {
        private readonly IPolicy policy;

        public BehaviourTree(string name, IPolicy policy = null) : base(name)
        {
            this.policy = policy ?? Policies.RunForever;
        }

        public override Status Process()
        {
            var status = this.children[this.currentChild].Process();
            if (this.policy.ShouldReturn(status)) return status;
            this.currentChild = (this.currentChild + 1) % this.children.Count;
            return Status.Running;
        }

        public void PrintTree()
        {
            var sb = new StringBuilder();
            PrintNode(this, 0, sb);
            Debug.Log(sb.ToString());
        }

        private static void PrintNode(Node node, int indentLevel, StringBuilder sb)
        {
            sb.Append(' ', indentLevel * 2).AppendLine(node.name);
            foreach (var child in node.children) PrintNode(child, indentLevel + 1, sb);
        }
    }
}