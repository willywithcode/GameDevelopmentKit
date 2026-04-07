#if LEADERBOARD
namespace GameFoundation.Scripts.Features.LeaderBoard.Services
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.LeaderBoard.LocalData;

    public interface ILeaderBoardProvider
    {
        UniTask<List<LeaderBoardEntry>> FetchEntries();
        UniTask SubmitScore(string playerId, string displayName, int avatarIndex, int score);
        void Dispose();
    }
}
#endif
