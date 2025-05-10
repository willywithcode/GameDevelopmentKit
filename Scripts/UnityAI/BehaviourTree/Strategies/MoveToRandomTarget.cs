namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Extenstions;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;
    using UnityEngine;
    using UnityEngine.AI;

    public class MoveToRandomTarget : IStrategy
    {
        private readonly Transform       entity;
        private readonly NavMeshAgent    agent;
        private readonly List<Transform> targets;
        private readonly float           speed;
        private          bool            isPathCalculated;
        private          Transform       target;

        public MoveToRandomTarget(Transform entity, NavMeshAgent agent, List<Transform> targets, float speed = 2)
        {
            this.entity  = entity;
            this.agent   = agent;
            this.targets = targets;
            this.speed   = speed;
            this.target  = targets[Random.Range(0, targets.Count)];
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
            this.target           = this.targets[Random.Range(0, this.targets.Count)];
            this.isPathCalculated = false;
        }
    }
}