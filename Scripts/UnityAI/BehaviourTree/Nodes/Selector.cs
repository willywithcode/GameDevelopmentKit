namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    public class Selector : Node {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (this.currentChild < this.children.Count) {
                switch (this.children[this.currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        this.Reset();
                        return Status.Success;
                    default:
                        this.currentChild++;
                        return Status.Running;
                }
            }
            
            this.Reset();
            return Status.Failure;
        }
    }
}