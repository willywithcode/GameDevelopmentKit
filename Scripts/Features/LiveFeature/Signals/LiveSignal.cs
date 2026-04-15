namespace GameFoundation.Scripts.Features.LiveFeature.Signals
{
    public readonly struct OnLivesChanged
    {
        public int  CurrentLives { get; }
        public int  MaxLives     { get; }
        public bool IsInfinity   { get; }

        public OnLivesChanged(int currentLives, int maxLives, bool isInfinity)
        {
            this.CurrentLives = currentLives;
            this.MaxLives     = maxLives;
            this.IsInfinity   = isInfinity;
        }
    }

    public readonly struct OnLivesTimerTick
    {
        public int  SecondsUntilNextLife { get; }
        public bool IsInfinityActive     { get; }
        public int  InfinitySecondsLeft  { get; }

        public OnLivesTimerTick(int secondsUntilNextLife, bool isInfinityActive, int infinitySecondsLeft)
        {
            this.SecondsUntilNextLife = secondsUntilNextLife;
            this.IsInfinityActive     = isInfinityActive;
            this.InfinitySecondsLeft  = infinitySecondsLeft;
        }
    }
}
