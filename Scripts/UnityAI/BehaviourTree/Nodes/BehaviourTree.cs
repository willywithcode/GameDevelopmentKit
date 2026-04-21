namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    using System.Text;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Policies;

    public class BehaviourTree : Node
    {
        private static readonly IGameLogger Logger = new LoggerService();
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
            Logger.Info(sb.ToString());
        }

        private static void PrintNode(Node node, int indentLevel, StringBuilder sb)
        {
            sb.Append(' ', indentLevel * 2).AppendLine(node.name);
            foreach (var child in node.children) PrintNode(child, indentLevel + 1, sb);
        }
    }
}
