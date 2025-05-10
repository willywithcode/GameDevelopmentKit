namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Strategies
{
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;

    public interface IStrategy
    {
        Node.Status Process();

        void Reset()
        {
            // Noop
        }
    }
}