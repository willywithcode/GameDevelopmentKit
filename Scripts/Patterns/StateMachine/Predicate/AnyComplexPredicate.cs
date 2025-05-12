namespace GameFoundation.Scripts.Patterns.StateMachine.Predicate
{
    using ZLinq;

    public class AnyComplexPredicate : IPredicate
    {
        private readonly IPredicate[] predicates;

        public AnyComplexPredicate(IPredicate[] predicates)
        {
            this.predicates = predicates;
        }

        public bool Evaluate()
        {
            return this.predicates.AsValueEnumerable().Any(predicate => predicate.Evaluate());
        }
    }
}