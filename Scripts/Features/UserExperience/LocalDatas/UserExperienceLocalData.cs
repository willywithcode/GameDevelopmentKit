namespace GameFoundation.Scripts.Features.UserExperience.LocalDatas
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class UserExperienceLocalData : ILocalData
    {
        public int    timePlayed = 0;
        public string GetKey() => this.GetType().ToString();

        public void Reset()
        {
            this.timePlayed = 0;
        }
    }

    public class UserExperienceLocalDataService : BaseLocalDataService<UserExperienceLocalData>
    {
        public void EnterGame()
        {
            this.Data.timePlayed++;
            this.Save();
        }

        public int GetTimePlayed()
        {
            return this.Data.timePlayed;
        }
    }
}