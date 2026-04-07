#if LEADERBOARD
namespace GameFoundation.Scripts.Features.LeaderBoard.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Cysharp.Threading.Tasks.Linq;
    using GameFoundation.Scripts.Features.LeaderBoard.Blueprints;
    using GameFoundation.Scripts.Features.LeaderBoard.LocalData;

    public class LocalLeaderBoardProvider : ILeaderBoardProvider
    {
        private readonly LeaderBoardLocalDataService localDataService;
        private readonly LeaderBoardBlueprintService blueprintService;
        private readonly Random                      random = new();

        private CancellationTokenSource onlineTickCts;

        public event Action EntriesChanged;

        public LocalLeaderBoardProvider(
            LeaderBoardLocalDataService localDataService,
            LeaderBoardBlueprintService blueprintService
        )
        {
            this.localDataService = localDataService;
            this.blueprintService = blueprintService;
        }

        public void Initialize(string playerId, string displayName, int avatarIndex, int bestScore)
        {
            this.EnsurePopulated(playerId, displayName, avatarIndex, bestScore);
            this.ApplyOfflineGains();
            this.StartOnlineTick();
        }

        public UniTask<List<LeaderBoardEntry>> FetchEntries()
        {
            return UniTask.FromResult(new List<LeaderBoardEntry>(this.localDataService.Entries));
        }

        public UniTask SubmitScore(string playerId, string displayName, int avatarIndex, int score)
        {
            var entries     = this.localDataService.Entries;
            var playerEntry = entries.FirstOrDefault(e => e.IsPlayer);

            if (playerEntry != null)
            {
                if (score <= playerEntry.Score) return UniTask.CompletedTask;
                playerEntry.Score       = score;
                playerEntry.DisplayName = displayName;
                playerEntry.AvatarIndex = avatarIndex;
            }
            else
            {
                entries.Add(new LeaderBoardEntry
                {
                    PlayerId    = playerId,
                    DisplayName = displayName,
                    AvatarIndex = avatarIndex,
                    Score       = score,
                    IsPlayer    = true,
                });
            }

            this.SortAndSave();
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            this.onlineTickCts?.Cancel();
            this.onlineTickCts?.Dispose();
            this.onlineTickCts = null;
            this.localDataService.TouchTimestamp();
        }

        private void EnsurePopulated(string playerId, string displayName, int avatarIndex, int bestScore)
        {
            if (this.localDataService.Entries.Count > 0) return;

            var bp      = this.blueprintService.GetBlueprint();
            var entries = new List<LeaderBoardEntry>();

            entries.Add(new LeaderBoardEntry
            {
                PlayerId    = playerId,
                DisplayName = displayName,
                AvatarIndex = avatarIndex,
                Score       = bestScore,
                IsPlayer    = true,
            });

            var botCount  = Math.Min(bp.BotCount, bp.BotNames.Length);
            var usedNames = new HashSet<int>();
            for (var i = 0; i < botCount; i++)
            {
                int nameIndex;
                do { nameIndex = this.random.Next(bp.BotNames.Length); } while (!usedNames.Add(nameIndex));

                entries.Add(new LeaderBoardEntry
                {
                    PlayerId    = Guid.NewGuid().ToString("N"),
                    DisplayName = bp.BotNames[nameIndex],
                    AvatarIndex = this.random.Next(0, 9),
                    Score       = this.random.Next(bp.InitialScoreMin, bp.InitialScoreMax + 1),
                    IsPlayer    = false,
                });
            }

            entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            this.localDataService.SetEntries(entries);
        }

        private void ApplyOfflineGains()
        {
            var lastUpdate = this.localDataService.LastUpdatedAtUtc;
            if (lastUpdate == DateTime.MinValue) return;

            var bp      = this.blueprintService.GetBlueprint();
            var elapsed = DateTime.UtcNow - lastUpdate;

            var maxOffline = TimeSpan.FromHours(bp.MaxOfflineHours);
            if (elapsed > maxOffline) elapsed = maxOffline;
            if (elapsed.TotalSeconds < bp.OfflineIntervalSeconds) return;

            var ticks = (int)(elapsed.TotalSeconds / bp.OfflineIntervalSeconds);

            foreach (var entry in this.localDataService.Entries.Where(e => !e.IsPlayer))
            {
                var totalGain = 0;
                for (var t = 0; t < ticks; t++)
                    totalGain += this.random.Next(bp.OfflineScoreGainMin, bp.OfflineScoreGainMax + 1);

                entry.Score = Math.Max(entry.Score + totalGain, 0);
            }

            this.SortAndSave();
        }

        private void StartOnlineTick()
        {
            this.onlineTickCts?.Cancel();
            this.onlineTickCts = new CancellationTokenSource();
            this.OnlineTickLoop(this.onlineTickCts.Token).Forget();
        }

        private async UniTaskVoid OnlineTickLoop(CancellationToken ct)
        {
            var bp         = this.blueprintService.GetBlueprint();
            var intervalMs = (int)(bp.OnlineIntervalSeconds * 1000);

            await foreach (var _ in UniTaskAsyncEnumerable.Timer(
                               TimeSpan.FromMilliseconds(intervalMs),
                               TimeSpan.FromMilliseconds(intervalMs))
                           .WithCancellation(ct))
            {
                this.ApplyOnlineTick();
            }
        }

        private void ApplyOnlineTick()
        {
            var bp = this.blueprintService.GetBlueprint();

            foreach (var entry in this.localDataService.Entries.Where(e => !e.IsPlayer))
            {
                var gain = this.random.Next(bp.OnlineScoreGainMin, bp.OnlineScoreGainMax + 1);
                entry.Score = Math.Max(entry.Score + gain, 0);
            }

            this.SortAndSave();
        }

        private void SortAndSave()
        {
            var entries = this.localDataService.Entries;
            entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            this.localDataService.SetEntries(entries);
            this.EntriesChanged?.Invoke();
        }
    }
}
#endif
