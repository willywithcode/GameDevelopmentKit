namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using GameFoundation.Scripts.Extenstions;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;
    using UnityEngine;
    using UnityEngine.AI;

    public class MoveToTarget : IStrategy
    {
        private readonly Transform    entity;
        private readonly NavMeshAgent agent;
        private readonly Transform    target;
        private readonly float        speed;
        private          bool         isPathCalculated;

        public MoveToTarget(Transform entity, NavMeshAgent agent, Transform target, float speed = 2)
        {
            this.entity = entity;
            this.agent  = agent;
            this.target = target;
            this.speed  = speed;
        }

        public Node.Status Process()
        {
            if (Vector3.Distance(this.entity.position, this.target.position) < 1f) return Node.Status.Success;

            this.agent.SetDestination(this.target.position);
            this.agent.speed = this.speed;
            this.entity.LookAt(this.target.position.With(y: this.entity.position.y));

            if (this.agent.pathPending) this.isPathCalculated = true;
            return Node.Status.Running;
        }

        public void Reset()
        {
            this.isPathCalculated = false;
        }
    }
}