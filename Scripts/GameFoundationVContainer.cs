namespace GameFoundation.Scripts
{
    #if DAILY_REWARD
    using HGameFoundation.Scripts.Features.DailyReward.DI;
    #endif
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.DI;
    using GameFoundation.Scripts.EntityManager.DI;
    using GameFoundation.Scripts.Features.AudioSystem.DI;
    using GameFoundation.Scripts.Features.InternetChecking.DI;
    using GameFoundation.Scripts.Features.Inventory.DI;
    using GameFoundation.Scripts.Features.LiveFeature.DI;
    using GameFoundation.Scripts.Features.NewDayReset.DI;
    using GameFoundation.Scripts.Features.UserExperience.DI;
    using GameFoundation.Scripts.Features.Vibration.DI;
    using GameFoundation.Scripts.LocalData.DI;
    using GameFoundation.Scripts.Patterns.MVP.DI;
    using GameFoundation.Scripts.Patterns.ObjectPooling;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using VContainer;
    #if MULTILANGUAGE
    using GameFoundation.Scripts.Features.Language.DI;
    #endif

    #if STATE_MACHINE
    using GameFoundation.Scripts.Patterns.StateMachine.DI;
    #endif
    #if SCHEDULE_REWARD
    using GameFoundation.Scripts.Features.ScheduleReward.DI;
    #endif

    public static class GameFoundationVContainer
    {
        public static void RegisterGameFoundation(this IContainerBuilder builder)
        {
            builder.Register<AssetsManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ObjectPoolManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SignalBus>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterMVP();
            builder.RegisterEntityManager();
            builder.RegisterNewDayReset();
            builder.RegisterAudio();
            builder.RegisterLocalData();
            builder.RegisterVibration();
            builder.RegisterInternetChecking();
            builder.RegisterUserExperience();
            builder.RegisterInventory();
            builder.RegisterSOBlueprint();
            #if DAILY_REWARD
            builder.RegisterDailyReward();
            #endif
            #if STATE_MACHINE
            builder.RegisterStateMachine();
            #endif
            #if MULTILANGUAGE
            builder.RegisterLanguage();
            #endif
            #if SCHEDULE_REWARD
            builder.RegisterScheduleReward();
            #endif
            #if LIVES
            builder.RegisterLives();
            #endif
        }
    }
}