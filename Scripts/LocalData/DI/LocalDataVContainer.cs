namespace GameFoundation.Scripts.LocalData.DI
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.Utils;
    using VContainer;

    public static class LocalDataVContainer
    {
        public static void RegisterLocalData(this IContainerBuilder builder)
        {
            var localDataServiceType = typeof(ILocalDataService<>);
            var serviceTypes         = ReflectionExtensions.GetAllImplementationsOfGenericInterface(localDataServiceType);

            foreach (var serviceType in serviceTypes)
                builder.Register(serviceType, Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}