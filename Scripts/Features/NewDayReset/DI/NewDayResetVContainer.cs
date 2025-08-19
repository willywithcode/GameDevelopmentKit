namespace GameFoundation.Scripts.Features.NewDayReset.DI
{
    using GameFoundation.Scripts.Features.NewDayReset.Services;
    using VContainer;

    public static class NewDayResetVContainer
    {
        public static void RegisterNewDayReset(this IContainerBuilder builder)
        {
            builder.Register<NewDayResetService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}