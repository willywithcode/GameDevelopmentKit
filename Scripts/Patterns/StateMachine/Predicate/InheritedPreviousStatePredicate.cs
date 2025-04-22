namespace GameFoundation.Scripts.Patterns.StateMachine.Predicate
{
    using GameFoundation.Scripts.Patterns.StateMachine.States;

    public class InheritedPreviousStatePredicate
    {
        private readonly StateMachine stateMachine;
        private readonly IState       state;

        public InheritedPreviousStatePredicate(StateMachine stateMachine, IState state)
        {
            this.stateMachine = stateMachine;
            this.state        = state;
        }

        public bool Evaluate()
        {
            if (this.stateMachine.Previous == null)
            {
                return false;
            }
            return this.state.GetType() == this.stateMachine.Previous.State.GetType() || this.stateMachine.Previous.State.GetType().IsSubclassOf(this.state.GetType());
        }
    }
}