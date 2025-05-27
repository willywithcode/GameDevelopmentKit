namespace GameFoundation.Scripts.Features.Vibration.DI
{
    using GameFoundation.Scripts.Features.Vibration.Services;
    using VContainer;

    public static class VibrationVContainer
    {
        public static void RegisterVibration(this IContainerBuilder builder)
        {
            builder.Register<VibrationService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}