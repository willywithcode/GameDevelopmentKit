namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Policies
{
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;

    public interface IPolicy
    {
        bool ShouldReturn(Node.Status status);
    }

    public static class Policies
    {
        public static readonly IPolicy RunForever      = new RunForeverPolicy();
        public static readonly IPolicy RunUntilSuccess = new RunUntilSuccessPolicy();
        public static readonly IPolicy RunUntilFailure = new RunUntilFailurePolicy();

        class RunForeverPolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => false;
        }

        class RunUntilSuccessPolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Success;
        }

        class RunUntilFailurePolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Failure;
        }
    }
}