namespace GameFoundation.Scripts.EntityManager.DI
{
    using GameFoundation.Scripts.EntityManager.Core;
    using VContainer;
    using VContainer.Unity;

    public static class EntityVContainer
    {
        public static void RegisterEntityManager(this IContainerBuilder builder)
        {
            builder.Register<EntityManager>(Lifetime.Singleton)
                   .As<IEntityManager>()
                   .As<ITickable>();
        }
    }
}