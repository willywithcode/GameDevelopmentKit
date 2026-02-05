namespace GameFoundation.Scripts.Features.LiveFeature.Signals
{
    public class OnLivesChanged
    {
        public int  CurrentLives;
        public int  MaxLives;
        public bool IsInfinity;
    }

    public class OnLivesTimerTick
    {
        public int  SecondsUntilNextLife;
        public bool IsInfinityActive;
        public int  InfinitySecondsLeft;
    }
}
