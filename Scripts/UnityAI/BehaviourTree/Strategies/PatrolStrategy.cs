namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Extenstions;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;
    using UnityEngine;
    using UnityEngine.AI;

    public class PatrolStrategy : IStrategy
    {
        private readonly Transform       entity;
        private readonly NavMeshAgent    agent;
        private readonly List<Transform> patrolPoints;
        private readonly float           patrolSpeed;
        private          int             currentIndex;
        private          bool            isPathCalculated;

        public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 2f)
        {
            this.entity       = entity;
            this.agent        = agent;
            this.patrolPoints = patrolPoints;
            this.patrolSpeed  = patrolSpeed;
        }

        public Node.Status Process()
        {
            if (this.currentIndex == this.patrolPoints.Count) return Node.Status.Success;

            var target = this.patrolPoints[this.currentIndex];
            this.agent.SetDestination(target.position);
            this.agent.speed = this.patrolSpeed;
            this.entity.LookAt(target.position.With(y: this.entity.position.y));

            if (this.isPathCalculated && this.agent.remainingDistance < 0.1f)
            {
                this.currentIndex++;
                this.isPathCalculated = false;
            }

            if (this.agent.pathPending) this.isPathCalculated = true;

            return Node.Status.Running;
        }

        public void Reset()
        {
            this.currentIndex = 0;
        }
    }
}