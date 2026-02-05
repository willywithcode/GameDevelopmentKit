namespace GameFoundation.Scripts.Features.LiveFeature.DI
{
    using GameFoundation.Scripts.Features.LiveFeature.Services;
    using VContainer;

    public static class LiveVContainer
    {
        public static void RegisterLives(this IContainerBuilder builder)
        {
            builder.Register<LiveService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}