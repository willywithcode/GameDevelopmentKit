namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using System;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;

    public class WaitUntil : IStrategy
    {
        private readonly Func<bool> predicate;

        public WaitUntil(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public Node.Status Process()
        {
            return this.predicate() ? Node.Status.Success : Node.Status.Running;
        }
    }
}