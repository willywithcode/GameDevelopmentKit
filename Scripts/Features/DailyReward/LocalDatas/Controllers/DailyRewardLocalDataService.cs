namespace GameFoundation.Scripts.Features.DailyReward.LocalDatas.Controllers
{
    using System;
    using GameFoundation.Scripts.Features.DailyReward.LocalDatas.Models;
    using GameFoundation.Scripts.LocalData.Service;

    public class DailyRewardLocalDataService : BaseLocalDataService<DailyRewardLocalData>
    {
        public int LastRewardIndex
        {
            get => this.Data.LastRewardIndex;
            set
            {
                this.Data.LastRewardIndex = value;
                this.Save();
            }
        }

        public DateTime LastRewardDate => this.Data.LastRewardDate;

        public void SetLastRewardDate()
        {
            this.Data.LastRewardDate = DateTime.Now;
            this.Save();
        }
    }
}