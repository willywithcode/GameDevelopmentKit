namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    public class UntilFail : Node {
        public UntilFail(string name) : base(name) { }
        
        public override Status Process() {
            if (this.children[0].Process() == Status.Failure) {
                this.Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }
}