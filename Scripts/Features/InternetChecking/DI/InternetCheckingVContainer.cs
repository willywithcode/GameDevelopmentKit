namespace GameFoundation.Scripts.Features.InternetChecking.DI
{
    using GameFoundation.Scripts.Features.InternetChecking.Services;
    using VContainer;

    public static class InternetCheckingVContainer
    {
        public static void RegisterInternetChecking(this IContainerBuilder builder)
        {
            builder.Register<UnityInternetReachability>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AndroidNetworkSettingsOpener>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<InternetCheckingService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }

        /// <summary>Blocks play without internet. Requires <see cref="RegisterInternetChecking"/>.</summary>
        public static void RegisterInternetRequired(this IContainerBuilder builder)
        {
            builder.Register<InternetRequiredService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
