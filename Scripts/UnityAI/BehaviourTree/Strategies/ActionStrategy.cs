namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using System;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;

    public class ActionStrategy : IStrategy
    {
        private readonly Action doSomething;

        public ActionStrategy(Action doSomething)
        {
            this.doSomething = doSomething;
        }

        public Node.Status Process()
        {
            this.doSomething();
            return Node.Status.Success;
        }
    }
}