namespace GameFoundation.Scripts.Features.LeaderBoard.LocalData
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    [Serializable]
    public class LeaderBoardEntry
    {
        public string PlayerId      { get; set; } = string.Empty;
        public string DisplayName   { get; set; } = string.Empty;
        public int    AvatarIndex   { get; set; }
        public string AvatarAddress { get; set; } = string.Empty;
        public int    Score         { get; set; }
        public bool   IsPlayer     { get; set; }
    }

    public class LeaderBoardLocalData : ILocalData
    {
        public List<LeaderBoardEntry> Entries         { get; set; } = new();
        public DateTime               LastUpdatedAtUtc { get; set; } = DateTime.MinValue;

        public string GetKey() => this.GetType().ToString();

        public void Reset()
        {
            this.Entries.Clear();
            this.LastUpdatedAtUtc = DateTime.MinValue;
        }
    }

    public class LeaderBoardLocalDataService : ACTkBaseLocalDataService<LeaderBoardLocalData>
    {
        public List<LeaderBoardEntry> Entries          => this.Data.Entries;
        public DateTime               LastUpdatedAtUtc => this.Data.LastUpdatedAtUtc;

        public void SetEntries(List<LeaderBoardEntry> entries)
        {
            this.Data.Entries          = entries;
            this.Data.LastUpdatedAtUtc = DateTime.UtcNow;
            this.Save();
        }

        public void TouchTimestamp()
        {
            this.Data.LastUpdatedAtUtc = DateTime.UtcNow;
            this.Save();
        }
    }
}
