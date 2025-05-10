namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies;

    public class Leaf : Node
    {
        private readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            // Preconditions.CheckNotNull(strategy);
            this.strategy = strategy;
        }

        public override Status Process()
        {
            return this.strategy.Process();
        }

        public override void Reset()
        {
            this.strategy.Reset();
        }
    }
}