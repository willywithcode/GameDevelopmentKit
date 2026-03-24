namespace GameFoundation.Scripts.Features.Profile.Services
{
    using System;
    using GameFoundation.Scripts.Features.Profile.LocalData;
    using GameFoundation.Scripts.Features.Profile.Signals;
    using GameFoundation.Scripts.Patterns.SignalBus;

    public class ProfileService
    {
        private const string DefaultDisplayName = "Player";
        private const int MaxDisplayNameLength = 16;
        private const int DefaultAvatarIndex = 0;
        private const int AvatarCount = 10;
        private const string AvatarAddressPrefix = "ProfileAvatar_";

        private readonly ProfileLocalDataService profileLocalDataService;
        private readonly SignalBus signalBus;

        public ProfileService(
            ProfileLocalDataService profileLocalDataService,
            SignalBus               signalBus
        )
        {
            this.profileLocalDataService = profileLocalDataService;
            this.signalBus               = signalBus;

            this.EnsureProfileCreated();
        }

        public event Action ProfileChanged;

        public string PlayerId => this.profileLocalDataService.PlayerId;
        public string DisplayName => this.profileLocalDataService.DisplayName;
        public int AvatarIndex => this.profileLocalDataService.AvatarIndex;
        public int GamesPlayed => this.profileLocalDataService.GamesPlayed;
        public int BestScore => this.profileLocalDataService.BestScore;
        public int HighestUnlockedLevel => this.profileLocalDataService.HighestUnlockedLevel;
        public DateTime CreatedAtUtc => this.profileLocalDataService.CreatedAtUtc;
        public DateTime LastUpdatedAtUtc => this.profileLocalDataService.LastUpdatedAtUtc;

        public string GetAvatarAddress(int avatarIndex)
        {
            return $"{AvatarAddressPrefix}{this.GetSafeAvatarIndex(avatarIndex)}";
        }

        public void SetDisplayName(string displayName)
        {
            var sanitizedDisplayName = this.SanitizeDisplayName(displayName);
            if (!this.profileLocalDataService.SetDisplayName(sanitizedDisplayName)) return;
            this.PublishProfileChanged();
        }

        public void SetAvatarIndex(int avatarIndex)
        {
            var sanitizedAvatarIndex = Math.Max(0, avatarIndex);
            if (!this.profileLocalDataService.SetAvatarIndex(sanitizedAvatarIndex)) return;
            this.PublishProfileChanged();
        }

        public void SetProfile(string displayName, int avatarIndex)
        {
            var sanitizedDisplayName = this.SanitizeDisplayName(displayName);
            var sanitizedAvatarIndex = Math.Max(0, avatarIndex);
            if (!this.profileLocalDataService.SetProfile(sanitizedDisplayName, sanitizedAvatarIndex)) return;
            this.PublishProfileChanged();
        }

        public void RecordGamePlayed()
        {
            if (!this.profileLocalDataService.IncrementGamesPlayed()) return;
            this.PublishProfileChanged();
        }

        public void RecordScore(int score)
        {
            if (!this.profileLocalDataService.SetBestScore(Math.Max(0, score))) return;
            this.PublishProfileChanged();
        }

        public void UnlockLevel(int level)
        {
            if (!this.profileLocalDataService.UnlockLevel(level)) return;
            this.PublishProfileChanged();
        }

        private void EnsureProfileCreated()
        {
            if (this.profileLocalDataService.HasProfile) return;

            this.profileLocalDataService.CreateProfile(
                Guid.NewGuid().ToString("N"),
                this.BuildDefaultDisplayName(),
                DefaultAvatarIndex,
                DateTime.UtcNow
            );

            this.signalBus.Fire(new OnProfileCreated(
                this.PlayerId,
                this.DisplayName,
                this.AvatarIndex,
                this.GamesPlayed,
                this.BestScore,
                this.HighestUnlockedLevel
            ));
        }

        private void PublishProfileChanged()
        {
            this.ProfileChanged?.Invoke();
            this.signalBus.Fire(new OnProfileChanged(
                this.PlayerId,
                this.DisplayName,
                this.AvatarIndex,
                this.GamesPlayed,
                this.BestScore,
                this.HighestUnlockedLevel
            ));
        }

        private string SanitizeDisplayName(string displayName)
        {
            var sanitizedDisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();

            if (sanitizedDisplayName.Length > MaxDisplayNameLength)
            {
                sanitizedDisplayName = sanitizedDisplayName[..MaxDisplayNameLength].Trim();
            }

            return string.IsNullOrWhiteSpace(sanitizedDisplayName) ? this.BuildDefaultDisplayName() : sanitizedDisplayName;
        }

        private string BuildDefaultDisplayName()
        {
            var suffix = (int)(BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0) % 10000);
            return $"{DefaultDisplayName}{suffix:0000}";
        }

        private int GetSafeAvatarIndex(int avatarIndex)
        {
            return Math.Clamp(avatarIndex, 0, AvatarCount - 1);
        }
    }
}
