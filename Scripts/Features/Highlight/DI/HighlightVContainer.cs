namespace GameFoundation.Scripts.Features.Highlight.DI
{
    using GameFoundation.Scripts.Features.Highlight.Services;
    using VContainer;
    public static class HighlightVContainer
    {
        public static void RegisterHighlightFeature(this IContainerBuilder builder)
        {
            builder.Register<HighlightService>(Lifetime.Singleton).AsSelf();
        }
    }
}