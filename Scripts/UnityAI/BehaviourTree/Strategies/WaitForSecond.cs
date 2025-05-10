namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;
    using UnityEngine;

    public class WaitForSecond : IStrategy
    {
        private readonly float waitTime;
        private          float timeCount;

        public WaitForSecond(float waitTime)
        {
            this.waitTime = waitTime;
        }

        public Node.Status Process()
        {
            this.timeCount += Time.deltaTime;
            if (!(this.timeCount >= this.waitTime)) return Node.Status.Running;
            this.Reset();
            return Node.Status.Success;
        }

        public void Reset()
        {
            this.timeCount = 0;
        }
    }
}