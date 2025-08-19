namespace GameFoundation.Scripts.Features.UserExperience.Services
{
    using System;
    using GameFoundation.Scripts.Features.UserExperience.LocalDatas;
    using UnityEngine;
    using VContainer.Unity;

    public class UserExperienceService : IInitializable, IDisposable
    {
        private readonly UserExperienceLocalDataService userExperienceLocalDataService;

        public UserExperienceService(UserExperienceLocalDataService userExperienceLocalDataService)
        {
            this.userExperienceLocalDataService = userExperienceLocalDataService;
        }

        public void Initialize()
        {
            this.userExperienceLocalDataService.EnterGame();
            Application.quitting += this.Dispose;
        }

        public void Dispose()
        {
            this.userExperienceLocalDataService.ExitGame();
        }

        public int GetTimePlayed()
        {
            return this.userExperienceLocalDataService.GetTimePlayed();
        }

        public DateTime GetLastLoginDate()
        {
            return this.userExperienceLocalDataService.GetLastLoginDate();
        }

        public DateTime GetLastLogoutDate()
        {
            return this.userExperienceLocalDataService.Data.lastLogoutDate;
        }
    }
}