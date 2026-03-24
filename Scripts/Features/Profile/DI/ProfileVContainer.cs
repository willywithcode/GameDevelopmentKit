namespace GameFoundation.Scripts.Features.Profile.DI
{
    using GameFoundation.Scripts.Features.Profile.Services;
    using VContainer;

    public static class ProfileVContainer
    {
        public static void RegisterProfile(this IContainerBuilder builder)
        {
            builder.Register<ProfileService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
