namespace GameFoundation.Scripts.Features.LeaderBoard.Services
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.LeaderBoard.LocalData;
    using GameFoundation.Scripts.Features.LeaderBoard.Signals;
    using GameFoundation.Scripts.Features.Profile.Services;
    using MessagePipe;

    public class LeaderBoardService
    {
        private readonly ILeaderBoardProvider           provider;
        private readonly ProfileService                 profileService;
        private readonly IPublisher<OnLeaderBoardUpdated> leaderBoardUpdatedPublisher;

        private List<LeaderBoardEntry> cachedEntries = new();

        public LeaderBoardService(
            ILeaderBoardProvider            provider,
            ProfileService                  profileService,
            IPublisher<OnLeaderBoardUpdated> leaderBoardUpdatedPublisher
        )
        {
            this.provider                    = provider;
            this.profileService              = profileService;
            this.leaderBoardUpdatedPublisher = leaderBoardUpdatedPublisher;

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
            this.ResolveAvatarAddresses();
            this.leaderBoardUpdatedPublisher.Publish(new OnLeaderBoardUpdated(this.cachedEntries));
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

        private void ResolveAvatarAddresses()
        {
            foreach (var entry in this.cachedEntries)
            {
                // Only resolve if the provider didn't already set an address
                // (a remote provider might return full URLs directly)
                if (string.IsNullOrEmpty(entry.AvatarAddress))
                    entry.AvatarAddress = this.profileService.GetAvatarAddress(entry.AvatarIndex);
            }
        }

        private void OnProviderEntriesChanged()
        {
            this.RefreshEntries().Forget();
        }
    }
}
