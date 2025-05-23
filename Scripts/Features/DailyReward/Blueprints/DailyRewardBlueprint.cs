namespace GameFoundation.Scripts.Features.DailyReward.Blueprints
{
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "DailyRewardBlueprint", menuName = "HyperCasual/DailyRewardBlueprint")]
    public class DailyRewardBlueprint : ScriptableObject
    {
        public DailyRewardData[] rewards;
    }

    [Serializable]
    public class DailyRewardData
    {
        public string rewardName;
        public int    rewardAmount;
    }
}