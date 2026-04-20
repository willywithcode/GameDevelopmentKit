namespace GameFoundation.Scripts.Features.Logger.DI
{
    using GameFoundation.Scripts.Features.Logger.Services;
    using VContainer;

    public static class LoggerVContainer
    {
        public static void RegisterLogger(this IContainerBuilder builder)
        {
            builder.Register<LoggerService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
