namespace GameFoundation.Scripts.Features.ScheduleReward.Service
{
    using System;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.Inventory.Services;
    using GameFoundation.Scripts.Features.ScheduleReward.Blueprints;
    using GameFoundation.Scripts.Features.ScheduleReward.LocalDatas;

    public class ScheduleRewardService
    {
        private          ScheduleRewardBlueprint        scheduleRewardBlueprint;
        private readonly ScheduleRewardLocalDataService scheduleRewardLocalDataService;
        private readonly IInventoryService              inventoryService;
        private readonly IAssetsManager                 assetsManager;

        public ScheduleRewardService(
            ScheduleRewardLocalDataService scheduleRewardLocalDataService,
            IInventoryService              inventoryService,
            IAssetsManager                 assetsManager
        )
        {
            this.scheduleRewardLocalDataService = scheduleRewardLocalDataService;
            this.inventoryService               = inventoryService;
            this.assetsManager                  = assetsManager;
            this.scheduleRewardBlueprint        = this.assetsManager.LoadAsset<ScheduleRewardBlueprint>("ScheduleRewardBlueprint");
        }

        public DateTime GetLastRewardTime(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= this.scheduleRewardLocalDataService.Data.RewardClaimedDates.Count)
            {
                return DateTime.Now;
            }
            return this.scheduleRewardLocalDataService.GetRewardClaimedDates()[rewardIndex];
        }

        public void ClaimReward(int rewardIndex, int multiplier = 1)
        {
            if (rewardIndex < 0 || rewardIndex >= this.scheduleRewardBlueprint.Rewards.Count) return;
            var rewardData            = this.scheduleRewardBlueprint.Rewards[rewardIndex];
            var lastClaimedTime       = this.GetLastRewardTime(rewardIndex);
            var hoursSinceLastClaimed = (DateTime.Now - lastClaimedTime).TotalHours;
            if (hoursSinceLastClaimed < rewardData.hoursToWait) return;
            foreach (var item in rewardData.items)
            {
                this.inventoryService.AddItem(item.RewardId, item.quantity * multiplier);
            }
            this.scheduleRewardLocalDataService.SetRewardClaimedDate(rewardIndex, DateTime.Now);
        }

        public bool CanClaimReward(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= this.scheduleRewardBlueprint.Rewards.Count) return false;
            var lastClaimedTime       = this.GetLastRewardTime(rewardIndex);
            var hoursSinceLastClaimed = (DateTime.Now - lastClaimedTime).TotalHours;
            return hoursSinceLastClaimed >= this.scheduleRewardBlueprint.Rewards[rewardIndex].hoursToWait;
        }

        public ScheduleRewardData GetRewardData(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= this.scheduleRewardBlueprint.Rewards.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardIndex), "Invalid reward index.");
            }
            return this.scheduleRewardBlueprint.Rewards[rewardIndex];
        }
    }
}