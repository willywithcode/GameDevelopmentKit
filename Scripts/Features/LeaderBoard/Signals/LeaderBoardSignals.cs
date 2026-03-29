namespace GameFoundation.Scripts.Features.LeaderBoard.Signals
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Features.LeaderBoard.LocalData;

    public class OnLeaderBoardUpdated
    {
        public List<LeaderBoardEntry> Entries { get; }

        public OnLeaderBoardUpdated(List<LeaderBoardEntry> entries)
        {
            this.Entries = entries;
        }
    }
}
