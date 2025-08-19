namespace GameFoundation.Scripts.Features.UserExperience.LocalDatas
{
    using System;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class UserExperienceLocalData : ILocalData
    {
        public int      timePlayed     = 0;
        public DateTime lastLoginDate  = DateTime.MinValue;
        public DateTime lastLogoutDate = DateTime.MinValue;
        public string   GetKey() => this.GetType().ToString();

        public void Reset()
        {
            this.timePlayed     = 0;
            this.lastLoginDate  = DateTime.MinValue;
            this.lastLogoutDate = DateTime.MinValue;
        }
    }

    public class UserExperienceLocalDataService : BaseLocalDataService<UserExperienceLocalData>
    {
        public void EnterGame()
        {
            this.Data.timePlayed++;
            this.Data.lastLoginDate = DateTime.Now;
            this.Save();
        }

        public void ExitGame()
        {
            this.Data.lastLogoutDate = DateTime.Now;
            this.Save();
        }

        public int GetTimePlayed()
        {
            return this.Data.timePlayed;
        }

        public DateTime GetLastLoginDate()
        {
            return this.Data.lastLoginDate;
        }
    }
}