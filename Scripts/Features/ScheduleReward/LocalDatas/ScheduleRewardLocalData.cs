namespace GameFoundation.Scripts.Features.ScheduleReward.LocalDatas
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.ScheduleReward.Blueprints;
    using GameFoundation.Scripts.Features.UserExperience.Services;
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;
    using VContainer.Unity;

    public class ScheduleRewardLocalData : ILocalData
    {
        public List<DateTime> RewardClaimedDates { get; set; } = new();
        public string         GetKey()           => this.GetType().ToString();

        public void Reset()
        {
            this.RewardClaimedDates = new();
        }
    }

    public class ScheduleRewardLocalDataService : BaseLocalDataService<ScheduleRewardLocalData>, IInitializable
    {
        private readonly IAssetsManager        assetsManager;
        private readonly UserExperienceService userExperienceService;

        public ScheduleRewardLocalDataService(
            IAssetsManager        assetsManager,
            UserExperienceService userExperienceService
        )
        {
            this.assetsManager         = assetsManager;
            this.userExperienceService = userExperienceService;
        }

        public void Initialize()
        {
            if (this.userExperienceService.GetTimePlayed() > 0) return;
            var defaultData = this.assetsManager.LoadAsset<ScheduleRewardBlueprint>("ScheduleRewardBlueprint");
            foreach (var reward in defaultData.Rewards)
            {
                if (reward.hoursToWait <= 0) continue;
                var date = DateTime.Now.AddHours(-reward.hoursToWait);
                this.Data.RewardClaimedDates.Add(date);
            }
            this.Save();
        }

        public List<DateTime> GetRewardClaimedDates()
        {
            return this.Data.RewardClaimedDates;
        }

        public void SetRewardClaimedDate(int rewardIndex, DateTime date)
        {
            if (rewardIndex < 0 || rewardIndex >= this.Data.RewardClaimedDates.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardIndex), "Invalid reward index.");
            }
            this.Data.RewardClaimedDates[rewardIndex] = date;
            this.Save();
        }
    }
}