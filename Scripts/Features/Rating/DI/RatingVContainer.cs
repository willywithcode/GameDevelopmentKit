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
        /// <para>
        /// Registered through factories rather than by type, because the App Store fallback needs
        /// its numeric app id handed in - nothing can derive it from the bundle id.
        /// </para>
        /// </summary>
        /// <param name="appleAppId">
        /// The numeric App Store id, for the iOS store page fallback only. Left empty, the iOS
        /// sheet still works and the store page answers <c>false</c>.
        /// </param>
        public static void RegisterRating(this IContainerBuilder builder, string appleAppId = "")
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<IRatingService>(_ => new GooglePlayRatingService(), Lifetime.Singleton);
#elif UNITY_IOS && !UNITY_EDITOR
            builder.Register<IRatingService>(_ => new AppStoreRatingService(appleAppId), Lifetime.Singleton);
#else
            builder.Register<IRatingService>(_ => new DummyRatingService(), Lifetime.Singleton);
#endif
        }
    }
}
