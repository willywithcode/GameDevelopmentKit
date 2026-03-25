namespace GameFoundation.Scripts.Features.Profile.LocalData
{
    using System;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class ProfileLocalData : ILocalData
    {
        public string   PlayerId             { get; set; } = string.Empty;
        public string   DisplayName          { get; set; } = string.Empty;
        public int      AvatarIndex          { get; set; }
        public int      GamesPlayed          { get; set; }
        public int      BestScore            { get; set; }
        public int      HighestUnlockedLevel { get; set; } = 1;
        public DateTime CreatedAtUtc         { get; set; } = DateTime.MinValue;
        public DateTime LastUpdatedAtUtc     { get; set; } = DateTime.MinValue;

        public string GetKey() => this.GetType().ToString();

        public void Reset()
        {
            this.PlayerId             = string.Empty;
            this.DisplayName          = string.Empty;
            this.AvatarIndex          = 0;
            this.GamesPlayed          = 0;
            this.BestScore            = 0;
            this.HighestUnlockedLevel = 1;
            this.CreatedAtUtc         = DateTime.MinValue;
            this.LastUpdatedAtUtc     = DateTime.MinValue;
        }
    }

    public class ProfileLocalDataService : ACTkBaseLocalDataService<ProfileLocalData>
    {
        public bool HasProfile => !string.IsNullOrWhiteSpace(this.Data.PlayerId);

        public string PlayerId => this.Data.PlayerId;
        public string DisplayName => this.Data.DisplayName;
        public int AvatarIndex => this.Data.AvatarIndex;
        public int GamesPlayed => this.Data.GamesPlayed;
        public int BestScore => this.Data.BestScore;
        public int HighestUnlockedLevel => this.Data.HighestUnlockedLevel;
        public DateTime CreatedAtUtc => this.Data.CreatedAtUtc;
        public DateTime LastUpdatedAtUtc => this.Data.LastUpdatedAtUtc;

        public void CreateProfile(string playerId, string displayName, int avatarIndex, DateTime createdAtUtc)
        {
            this.Data.PlayerId             = playerId;
            this.Data.DisplayName          = displayName;
            this.Data.AvatarIndex          = avatarIndex;
            this.Data.GamesPlayed          = 0;
            this.Data.BestScore            = 0;
            this.Data.HighestUnlockedLevel = 1;
            this.Data.CreatedAtUtc         = createdAtUtc;
            this.Touch();
            this.Save();
        }

        public bool SetDisplayName(string displayName)
        {
            if (this.Data.DisplayName == displayName) return false;
            this.Data.DisplayName = displayName;
            this.Touch();
            this.Save();
            return true;
        }

        public bool SetAvatarIndex(int avatarIndex)
        {
            if (this.Data.AvatarIndex == avatarIndex) return false;
            this.Data.AvatarIndex = avatarIndex;
            this.Touch();
            this.Save();
            return true;
        }

        public bool SetProfile(string displayName, int avatarIndex)
        {
            var isChanged = false;

            if (this.Data.DisplayName != displayName)
            {
                this.Data.DisplayName = displayName;
                isChanged             = true;
            }

            if (this.Data.AvatarIndex != avatarIndex)
            {
                this.Data.AvatarIndex = avatarIndex;
                isChanged             = true;
            }

            if (!isChanged) return false;
            this.Touch();
            this.Save();
            return true;
        }

        public bool IncrementGamesPlayed()
        {
            this.Data.GamesPlayed++;
            this.Touch();
            this.Save();
            return true;
        }

        public bool SetBestScore(int bestScore)
        {
            if (bestScore <= this.Data.BestScore) return false;
            this.Data.BestScore = bestScore;
            this.Touch();
            this.Save();
            return true;
        }

        public bool UnlockLevel(int level)
        {
            var sanitizedLevel = Math.Max(1, level);
            if (sanitizedLevel <= this.Data.HighestUnlockedLevel) return false;
            this.Data.HighestUnlockedLevel = sanitizedLevel;
            this.Touch();
            this.Save();
            return true;
        }

        private void Touch()
        {
            this.Data.LastUpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
