namespace GameFoundation.Scripts.Features.NewDayReset.LocalDatas
{
    using System;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class NewDayResetLocalData : ILocalData
    {
        public DateTime lastResetDate = DateTime.MinValue;
        public string   GetKey() => this.GetType().ToString();

        public void Reset()
        {
            this.lastResetDate = DateTime.MinValue;
        }
    }

    public class NewDayResetLocalDataService : BaseLocalDataService<NewDayResetLocalData>
    {
        public DateTime LastResetDate => this.Data.lastResetDate;

        public void SetLastResetDate()
        {
            this.Data.lastResetDate = DateTime.Now;
            this.Save();
        }

        public bool IsNewDay()
        {
            return DateTime.Now.Day > this.Data.lastResetDate.Day;
        }
    }
}