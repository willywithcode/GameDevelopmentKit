namespace GameFoundation.Scripts.Patterns.StateMachine
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.StateMachine.Predicate;
    using GameFoundation.Scripts.Patterns.StateMachine.States;
    using GameFoundation.Scripts.Patterns.StateMachine.Transition;

    public class StateMachine
    {
        private          StateNode                   current;
        private          StateNode                   previous;
        private readonly Dictionary<Type, StateNode> nodes          = new();
        private readonly HashSet<ITransition>        anyTransitions = new();
        private          bool                        isRunning;

        public   bool      IsRunning { set => this.isRunning = value; }
        internal StateNode Current   => this.current;
        internal StateNode Previous  => this.previous;

        public void Tick()
        {
            if (!this.isRunning) return;
            var transition = this.GetTransition();
            if (transition != null) this.ChangeState(transition.To);
            this.current.State?.Tick();
        }

        public void FixedTick()
        {
            if (!this.isRunning) return;
            this.current.State?.FixedTick();
        }

        public void SetState(IState state)
        {
            this.current = this.nodes[state.GetType()];
            this.current.State?.OnEnter().Forget();
        }

        private void ChangeState(IState state)
        {
            if (state == this.current.State) return;
            var previousState = this.current.State;
            var nextState     = this.nodes[state.GetType()].State;
            previousState?.OnExit().Forget();
            nextState?.OnEnter().Forget();
            this.previous = this.current;
            this.current  = this.nodes[state.GetType()];
        }

        private ITransition GetTransition()
        {
            foreach (var transition in this.anyTransitions)
                if (transition.Condition.Evaluate())
                    return transition;

            foreach (var transition in this.current.Transitions)
                if (transition.Condition.Evaluate())
                    return transition;

            return null;
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            this.GetOrAddNode(from).AddTransition(this.GetOrAddNode(to).State, condition);
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            this.anyTransitions.Add(new Transition.Transition(this.GetOrAddNode(to).State, condition));
        }

        private StateNode GetOrAddNode(IState state)
        {
            var node = this.nodes.GetValueOrDefault(state.GetType());
            if (node == null)
            {
                node = new(state);
                this.nodes.Add(state.GetType(), node);
            }
            return node;
        }

        internal class StateNode
        {
            public IState               State       { get; }
            public HashSet<ITransition> Transitions { get; }

            public StateNode(IState state)
            {
                this.State       = state;
                this.Transitions = new();
            }

            public void AddTransition(IState to, IPredicate condition)
            {
                this.Transitions.Add(new Transition.Transition(to, condition));
            }
        }
    }
}