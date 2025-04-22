namespace GameFoundation.Scripts.Patterns.StateMachine.Transition
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Patterns.StateMachine.Predicate;
    using GameFoundation.Scripts.Patterns.StateMachine.States;

    public interface ITransition
    {
        IState           To        { get; }
        IPredicate Condition { get; }
    }
}