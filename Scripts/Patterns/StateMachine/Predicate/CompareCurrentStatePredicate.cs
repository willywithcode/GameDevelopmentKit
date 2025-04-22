namespace GameFoundation.Scripts.Patterns.StateMachine.Predicate
{
    using GameFoundation.Scripts.Patterns.StateMachine.States;

    public class CompareCurrentStatePredicate : IPredicate
    {
        private readonly StateMachine stateMachine;
        private readonly IState       state;

        public CompareCurrentStatePredicate(StateMachine stateMachine, IState state)
        {
            this.stateMachine = stateMachine;
            this.state        = state;
        }

        public bool Evaluate() => this.stateMachine.Current.State.GetType() == this.state.GetType();
    }
}