namespace GameFoundation.Scripts.UnityAI.BehaviourTree
{
    using System.Threading;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;

    public interface IBehaviourTreeManager
    {
        public void AddBehaviourTree(string    name, BehaviourTree behaviourTree);
        public void RemoveBehaviourTree(string name);
        public void Start(CancellationToken    token);
        public void Stop(string                name);
        public void StopAll();
    }
}