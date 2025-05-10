namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;
    using UnityEngine;

    public class WaitForRandomSecond : IStrategy
    {
        private readonly float maxWaitTime;
        private readonly float minWaitTime;
        private          float timeCount;
        private          float waitTime;

        public WaitForRandomSecond(float maxWait, float minWait)
        {
            this.maxWaitTime = maxWait;
            this.minWaitTime = minWait;
            this.waitTime    = Random.Range(this.minWaitTime, this.maxWaitTime);
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
            this.waitTime  = Random.Range(this.minWaitTime, this.maxWaitTime);
            this.timeCount = 0;
        }
    }
}