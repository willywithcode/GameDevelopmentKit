namespace GameFoundation.Scripts.Patterns.StateMachine.Predicate
{
    using ZLinq;

    public class ComplexPredicate : IPredicate
    {
        private readonly IPredicate[] predicates;

        public ComplexPredicate(IPredicate[] predicates)
        {
            this.predicates = predicates;
        }

        public bool Evaluate() => this.predicates.AsValueEnumerable().All(predicate => predicate.Evaluate());
    }
}