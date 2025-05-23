namespace GameFoundation.Scripts.Features.DailyReward.Services
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.DailyReward.Blueprints;
    using GameFoundation.Scripts.Features.DailyReward.LocalDatas.Controllers;
    using GameFoundation.Scripts.Features.Inventory.Services;
    using VContainer.Unity;
    using ZLinq;

    public class DailyRewardService : IInitializable
    {
        #region Inject

        private readonly DailyRewardLocalDataService dailyRewardLocalDataService;
        private readonly IAssetsManager              assetsManager;
        private readonly IInventoryService           inventoryService;

        public DailyRewardService(
            DailyRewardLocalDataService dailyRewardLocalDataService,
            IAssetsManager              assetsManager,
            IInventoryService           inventoryService
        )
        {
            this.dailyRewardLocalDataService = dailyRewardLocalDataService;
            this.assetsManager               = assetsManager;
            this.inventoryService            = inventoryService;
        }

        #endregion

        private DailyRewardBlueprint dailyRewardBlueprint;

        public void Initialize()
        {
            this.dailyRewardBlueprint = this.assetsManager.LoadAsset<DailyRewardBlueprint>("DailyRewardBlueprint");
        }

        public int GetCurrentRewardIndex() => (this.dailyRewardLocalDataService.LastRewardIndex + 1) % this.dailyRewardBlueprint.rewards.Length;

        public List<DailyRewardData> GetDailyRewardData() => this.dailyRewardBlueprint.rewards.AsValueEnumerable().ToList();

        public bool CanClaimReward() => this.dailyRewardLocalDataService.LastRewardDate.Date < DateTime.Now.Date;

        public void ClaimReward()
        {
            this.dailyRewardLocalDataService.SetLastRewardDate();
            this.dailyRewardLocalDataService.LastRewardIndex = (this.dailyRewardLocalDataService.LastRewardIndex + 1) % this.dailyRewardBlueprint.rewards.Length;
            var reward = this.dailyRewardBlueprint.rewards[this.dailyRewardLocalDataService.LastRewardIndex];
            this.inventoryService.AddItem(reward.rewardName, reward.rewardAmount);
        }
    }
}