namespace GameFoundation.Scripts.Features.LeaderBoard.DI
{
    using GameFoundation.Scripts.Features.LeaderBoard.Services;
    using VContainer;

    public static class LeaderBoardVContainer
    {
        public static void RegisterLeaderBoard(this IContainerBuilder builder)
        {
            // Swap LocalLeaderBoardProvider with a remote implementation to use a real backend.
            // e.g. builder.Register<FirebaseLeaderBoardProvider>(Lifetime.Singleton).As<ILeaderBoardProvider>();
            builder.Register<LocalLeaderBoardProvider>(Lifetime.Singleton).As<ILeaderBoardProvider>();
            builder.Register<LeaderBoardService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
