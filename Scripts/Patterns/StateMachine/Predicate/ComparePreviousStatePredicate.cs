namespace GameFoundation.Scripts.Patterns.StateMachine.Predicate
{
    using GameFoundation.Scripts.Patterns.StateMachine.States;

    public class ComparePreviousStatePredicate : IPredicate
    {
        private readonly StateMachine stateMachine;
        private readonly IState       state;

        public ComparePreviousStatePredicate(StateMachine stateMachine, IState state)
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
            return this.stateMachine.Previous.State.GetType() == this.state.GetType();
        }
    }
}