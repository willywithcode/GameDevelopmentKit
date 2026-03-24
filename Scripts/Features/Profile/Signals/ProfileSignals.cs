namespace GameFoundation.Scripts.Features.Profile.Signals
{
    public class OnProfileCreated
    {
        public string PlayerId             { get; }
        public string DisplayName          { get; }
        public int    AvatarIndex          { get; }
        public int    GamesPlayed          { get; }
        public int    BestScore            { get; }
        public int    HighestUnlockedLevel { get; }

        public OnProfileCreated(
            string playerId,
            string displayName,
            int avatarIndex,
            int gamesPlayed,
            int bestScore,
            int highestUnlockedLevel
        )
        {
            this.PlayerId             = playerId;
            this.DisplayName          = displayName;
            this.AvatarIndex          = avatarIndex;
            this.GamesPlayed          = gamesPlayed;
            this.BestScore            = bestScore;
            this.HighestUnlockedLevel = highestUnlockedLevel;
        }
    }

    public class OnProfileChanged
    {
        public string PlayerId             { get; }
        public string DisplayName          { get; }
        public int    AvatarIndex          { get; }
        public int    GamesPlayed          { get; }
        public int    BestScore            { get; }
        public int    HighestUnlockedLevel { get; }

        public OnProfileChanged(
            string playerId,
            string displayName,
            int avatarIndex,
            int gamesPlayed,
            int bestScore,
            int highestUnlockedLevel
        )
        {
            this.PlayerId             = playerId;
            this.DisplayName          = displayName;
            this.AvatarIndex          = avatarIndex;
            this.GamesPlayed          = gamesPlayed;
            this.BestScore            = bestScore;
            this.HighestUnlockedLevel = highestUnlockedLevel;
        }
    }
}
