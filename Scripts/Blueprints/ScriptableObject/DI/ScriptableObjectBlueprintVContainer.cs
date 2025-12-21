namespace GameFoundation.Scripts.Blueprints.ScriptableObject.DI
{
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Services;
    using GameFoundation.Scripts.Utils;
    using VContainer;
    public static class ScriptableObjectBlueprintVContainer
    {
        public static void RegisterSOBlueprint(this IContainerBuilder builder)
        {
            var localDataServiceType = typeof(BaseSOBlueprintService<>);
            var serviceTypes         = ReflectionExtensions.GetAllImplementationsOf(localDataServiceType);

            foreach (var serviceType in serviceTypes) builder.Register(serviceType, Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}