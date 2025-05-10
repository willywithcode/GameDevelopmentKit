namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    public class Sequence : Node {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (this.currentChild < this.children.Count) {
                switch (this.children[this.currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        this.currentChild = 0;
                        return Status.Failure;
                    default:
                        this.currentChild++;
                        return this.currentChild == this.children.Count ? Status.Success : Status.Running;
                }
            }

            this.Reset();
            return Status.Success;
        }
    }
}