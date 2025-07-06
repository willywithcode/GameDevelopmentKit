namespace GameFoundation.Scripts.Features.UserExperience.DI
{
    using GameFoundation.Scripts.Features.UserExperience.Services;
    using VContainer;

    public static class UserExperienceVContainer
    {
        public static void RegisterUserExperience(this IContainerBuilder builder)
        {
            builder.Register<UserExperienceService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}