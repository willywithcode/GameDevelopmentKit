namespace GameFoundation.Scripts.Features.DailyReward.LocalDatas.Models
{
    using System;
    using GameFoundation.Scripts.LocalData.Interfaces;

    public class DailyRewardLocalData : ILocalData
    {
        public int      LastRewardIndex { get; set; } = -1;
        public DateTime LastRewardDate  { get; set; } = DateTime.MinValue;
        public string   GetKey()        => this.GetType().ToString();

        public void Reset()
        {
            this.LastRewardIndex = -1;
            this.LastRewardDate  = DateTime.MinValue;
        }
    }
}