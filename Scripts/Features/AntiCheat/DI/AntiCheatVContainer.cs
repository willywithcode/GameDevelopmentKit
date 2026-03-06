namespace GameFoundation.Scripts.Features.AntiCheat.DI
{
    using GameFoundation.Scripts.Features.AntiCheat.Services;
    using VContainer;

    public static class AntiCheatVContainer
    {
        public static void RegisterAntiCheat(this IContainerBuilder builder)
        {
            builder.Register<AntiCheatService>(Lifetime.Singleton)
                   .AsSelf()
                   .AsImplementedInterfaces();
        }
    }
}
