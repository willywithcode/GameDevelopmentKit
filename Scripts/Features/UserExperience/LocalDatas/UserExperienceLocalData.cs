namespace GameFoundation.Scripts.Features.UserExperience.LocalDatas
{
    using System;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class UserExperienceLocalData : ILocalData
    {
        public int      sessionCount   = 0;
        public DateTime lastLoginDate  = DateTime.MinValue;
        public DateTime lastLogoutDate = DateTime.MinValue;
        public string   installVersion = string.Empty;
        public string   GetKey() => this.GetType().ToString();

        public void Reset()
        {
            this.sessionCount   = 0;
            this.lastLoginDate  = DateTime.MinValue;
            this.lastLogoutDate = DateTime.MinValue;
            this.installVersion = string.Empty;
        }
    }

    public class UserExperienceLocalDataService : BaseLocalDataService<UserExperienceLocalData>
    {
        public void EnterGame()
        {
            if (string.IsNullOrEmpty(this.Data.installVersion))
            {
                this.Data.installVersion = UnityEngine.Application.version;
            }
            this.Data.sessionCount++;
            this.Data.lastLoginDate = DateTime.Now;
            this.Save();
        }

        public int GetSessionCount()
        {
            return this.Data.sessionCount;
        }

        public string GetInstallVersion()
        {
            return this.Data.installVersion;
        }

        public bool IsUpdatedSinceInstall()
        {
            return !string.IsNullOrEmpty(this.Data.installVersion)
                && this.Data.installVersion != UnityEngine.Application.version;
        }

        public void ExitGame()
        {
            this.Data.lastLogoutDate = DateTime.Now;
            this.Save();
        }

        public DateTime GetLastLoginDate()
        {
            return this.Data.lastLoginDate;
        }
    }
}