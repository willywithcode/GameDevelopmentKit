namespace GameFoundation.Scripts.Features.UserExperience.Services
{
    using GameFoundation.Scripts.Features.UserExperience.LocalDatas;
    using VContainer.Unity;

    public class UserExperienceService : IInitializable
    {
        private readonly UserExperienceLocalDataService userExperienceLocalDataService;

        public UserExperienceService(UserExperienceLocalDataService userExperienceLocalDataService)
        {
            this.userExperienceLocalDataService = userExperienceLocalDataService;
        }

        public void Initialize()
        {
            this.userExperienceLocalDataService.EnterGame();
        }
        public int GetTimePlayed()
        {
            return this.userExperienceLocalDataService.GetTimePlayed();
        }
    }
}