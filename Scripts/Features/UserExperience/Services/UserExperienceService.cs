namespace GameFoundation.Scripts.Features.UserExperience.Services
{
    using System;
    using GameFoundation.Scripts.Features.UserExperience.LocalDatas;
    using UnityEngine;
    using VContainer.Unity;

    public class UserExperienceService : IPostInitializable, IDisposable
    {
        private readonly UserExperienceLocalDataService userExperienceLocalDataService;

        public UserExperienceService(UserExperienceLocalDataService userExperienceLocalDataService)
        {
            this.userExperienceLocalDataService = userExperienceLocalDataService;
        }

        public void PostInitialize()
        {
            this.userExperienceLocalDataService.EnterGame();
            Application.quitting += this.Dispose;
        }

        public void Dispose()
        {
            this.userExperienceLocalDataService.ExitGame();
        }

        public int GetSessionCount()
        {
            return this.userExperienceLocalDataService.GetSessionCount();
        }

        public DateTime GetLastLoginDate()
        {
            return this.userExperienceLocalDataService.GetLastLoginDate();
        }

        public DateTime GetLastLogoutDate()
        {
            return this.userExperienceLocalDataService.Data.lastLogoutDate;
        }

        public string GetInstallVersion()
        {
            return this.userExperienceLocalDataService.GetInstallVersion();
        }

        public bool IsUpdatedSinceInstall()
        {
            return this.userExperienceLocalDataService.IsUpdatedSinceInstall();
        }
    }
}