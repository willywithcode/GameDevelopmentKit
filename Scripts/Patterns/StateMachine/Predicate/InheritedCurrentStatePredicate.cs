namespace GameFoundation.Scripts.Patterns.StateMachine.Predicate
{
    using GameFoundation.Scripts.Patterns.StateMachine.States;

    public class InheritedCurrentStatePredicate : IPredicate
    {
        private readonly StateMachine stateMachine;
        private readonly IState       state;

        public InheritedCurrentStatePredicate(StateMachine stateMachine, IState state)
        {
            this.stateMachine = stateMachine;
            this.state        = state;
        }

        public bool Evaluate() => this.state.GetType() == this.stateMachine.Current.State.GetType() || this.stateMachine.Current.State.GetType().IsSubclassOf(this.state.GetType());
    }
}