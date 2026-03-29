namespace GameFoundation.Scripts.Features.LeaderBoard.Services
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.LeaderBoard.LocalData;
    using GameFoundation.Scripts.Features.LeaderBoard.Signals;
    using GameFoundation.Scripts.Features.Profile.Services;
    using GameFoundation.Scripts.Patterns.SignalBus;

    public class LeaderBoardService
    {
        private readonly ILeaderBoardProvider provider;
        private readonly ProfileService       profileService;
        private readonly SignalBus             signalBus;

        private List<LeaderBoardEntry> cachedEntries = new();

        public LeaderBoardService(
            ILeaderBoardProvider provider,
            ProfileService       profileService,
            SignalBus             signalBus
        )
        {
            this.provider       = provider;
            this.profileService = profileService;
            this.signalBus      = signalBus;

            if (this.provider is LocalLeaderBoardProvider localProvider)
            {
                localProvider.Initialize(
                    this.profileService.PlayerId,
                    this.profileService.DisplayName,
                    this.profileService.AvatarIndex,
                    this.profileService.BestScore
                );
                localProvider.EntriesChanged += this.OnProviderEntriesChanged;
            }

            this.RefreshEntries().Forget();
        }

        public List<LeaderBoardEntry> Entries => this.cachedEntries;

        public async UniTask RefreshEntries()
        {
            this.cachedEntries = await this.provider.FetchEntries();
            this.signalBus.Fire(new OnLeaderBoardUpdated(this.cachedEntries));
        }

        public async UniTask SubmitScore(int score)
        {
            await this.provider.SubmitScore(
                this.profileService.PlayerId,
                this.profileService.DisplayName,
                this.profileService.AvatarIndex,
                score
            );
            await this.RefreshEntries();
        }

        public int GetPlayerRank()
        {
            for (var i = 0; i < this.cachedEntries.Count; i++)
            {
                if (this.cachedEntries[i].IsPlayer) return i + 1;
            }

            return this.cachedEntries.Count + 1;
        }

        public void Dispose()
        {
            if (this.provider is LocalLeaderBoardProvider localProvider)
                localProvider.EntriesChanged -= this.OnProviderEntriesChanged;

            this.provider.Dispose();
        }

        private void OnProviderEntriesChanged()
        {
            this.RefreshEntries().Forget();
        }
    }
}
