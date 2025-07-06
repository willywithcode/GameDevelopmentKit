namespace GameFoundation.Scripts.Features.InternetChecking.DI
{
    using GameFoundation.Scripts.Features.InternetChecking.Services;
    using VContainer;

    public static class InternetCheckingVContainer
    {
        public static void RegisterInternetChecking(this IContainerBuilder builder)
        {
            builder.Register<InternetCheckingService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}