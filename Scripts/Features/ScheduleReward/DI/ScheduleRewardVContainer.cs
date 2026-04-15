namespace GameFoundation.Scripts.Features.ScheduleReward.DI
{
    using GameFoundation.Scripts.Features.ScheduleReward.Service;
    using VContainer;

    public static class ScheduleRewardVContainer
    {
        public static void RegisterScheduleReward(this IContainerBuilder builder)
        {
#if SCHEDULE_REWARD
            builder.Register<ScheduleRewardService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
#endif
        }
    }
}