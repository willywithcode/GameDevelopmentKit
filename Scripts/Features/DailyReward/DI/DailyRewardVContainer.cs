namespace HGameFoundation.Scripts.Features.DailyReward.DI
{
    using GameFoundation.Scripts.Features.DailyReward.Services;
    using VContainer;

    public static class DailyRewardVContainer
    {
        public static void RegisterDailyReward(this IContainerBuilder builder)
        {
            builder.Register<DailyRewardService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}