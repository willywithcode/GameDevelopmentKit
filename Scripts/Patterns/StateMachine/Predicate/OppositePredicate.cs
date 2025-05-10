namespace GameFoundation.Scripts.Patterns.StateMachine.Predicate
{
    public class OppositePredicate : IPredicate
    {
        private readonly IPredicate predicate;

        public OppositePredicate(IPredicate predicate)
        {
            this.predicate = predicate;
        }

        public bool Evaluate() => !this.predicate.Evaluate();
    }
}