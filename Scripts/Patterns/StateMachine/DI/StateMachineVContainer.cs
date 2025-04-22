namespace GameFoundation.Scripts.Patterns.StateMachine.DI
{
    using VContainer;

    public static class StateMachineVContainer
    {
        public static void RegisterStateMachine(this IContainerBuilder builder)
        {
            builder.Register<StateMachineManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}