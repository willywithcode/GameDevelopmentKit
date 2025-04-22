namespace GameFoundation.Scripts.Patterns.StateMachine.Transition
{
    using GameFoundation.Scripts.Patterns.StateMachine.Predicate;
    using GameFoundation.Scripts.Patterns.StateMachine.States;

    public class Transition : ITransition
    {
        public IState     To        { get; }
        public IPredicate Condition { get; }

        public Transition(IState to, IPredicate condition)
        {
            this.To        = to;
            this.Condition = condition;
        }
    }
}