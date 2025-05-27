namespace GameFoundation.Scripts
{
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.AudioSystem.DI;
    using GameFoundation.Scripts.Features.Vibration.DI;
    using GameFoundation.Scripts.LocalData.DI;
    using GameFoundation.Scripts.Patterns.MVP.DI;
    using GameFoundation.Scripts.Patterns.ObjectPooling;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using VContainer;
    #if STATE_MACHINE
    using GameFoundation.Scripts.Patterns.StateMachine.DI;
    #endif

    public static class GameFoundationVContainer
    {
        public static void RegisterGameFoundation(this IContainerBuilder builder)
        {
            builder.Register<AssetsManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ObjectPoolManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SignalBus>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterMVP();
            builder.RegisterAudio();
            builder.RegisterLocalData();
            builder.RegisterVibration();
            #if STATE_MACHINE
            builder.RegisterStateMachine();
            #endif
        }
    }
}