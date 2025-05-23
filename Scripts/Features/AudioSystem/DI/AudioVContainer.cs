namespace GameFoundation.Scripts.Features.AudioSystem.DI
{
    using GameFoundation.Scripts.Features.AudioSystem.Services;
    using GameFoundation.Scripts.Features.AudioSystem.SubServices;
    using VContainer;

    public static class AudioVContainer
    {
        public static void RegisterAudio(this IContainerBuilder builder)
        {
            builder.Register<AudioManagerService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<CommonButtonClickSound>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}