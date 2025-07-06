namespace GameFoundation.Scripts.Features.Language.DI
{
    using GameFoundation.Scripts.Features.Language.Services;
    using VContainer;

    public static class LanguageVContainer
    {
        public static void RegisterLanguage(this IContainerBuilder builder)
        {
            builder.Register<LanguageService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}