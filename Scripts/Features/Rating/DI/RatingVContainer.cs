namespace GameFoundation.Scripts.Features.Rating.DI
{
    using GameFoundation.Scripts.Features.Rating.Services;
    using VContainer;

    public static class RatingVContainer
    {
        /// <summary>
        /// Registers exactly one <see cref="IRatingService"/>: the store front this build ships to,
        /// or <see cref="DummyRatingService"/> everywhere else. Callers inject the interface and
        /// never learn which one they got.
        /// </summary>
        public static void RegisterRating(this IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<IRatingService>(_ => new GooglePlayRatingService(), Lifetime.Singleton);
#elif UNITY_IOS && !UNITY_EDITOR
            builder.Register<IRatingService>(_ => new AppStoreRatingService(), Lifetime.Singleton);
#else
            builder.Register<IRatingService>(_ => new DummyRatingService(), Lifetime.Singleton);
#endif
        }
    }
}
