namespace GameFoundation.Scripts.Features.PushNotification.DI
{
    using GameFoundation.Scripts.Features.PushNotification.DeepLink;
    using GameFoundation.Scripts.Features.PushNotification.Scheduler;
    using GameFoundation.Scripts.Features.PushNotification.Services;
    using VContainer;

    public static class PushVContainer
    {
        public static void RegisterPush(this IContainerBuilder builder)
        {
            #if PUSH_NOTIFICATION
            #if ONE_SIGNAL
            builder.Register<OneSignalPushService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #endif
            builder.Register<PushScheduler>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PushDeepLinkHandler>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #endif
        }
    }
}
