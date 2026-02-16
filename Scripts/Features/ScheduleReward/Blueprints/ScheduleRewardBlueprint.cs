namespace GameFoundation.Scripts.Features.ScheduleReward.Blueprints
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ScheduleRewardBlueprint", menuName = "GameFoundation/ScheduleRewardBlueprint", order = 1)]
    public class ScheduleRewardBlueprint : ScriptableObject
    {
        public List<ScheduleRewardData> Rewards;
    }

    [Serializable]
    public class ScheduleRewardData
    {
        public List<RewardItem>     items;
        public int                  hoursToWait;
        public TypeOfScheduleReward typeOfScheduleReward;
    }

    [Serializable]
    public class RewardItem
    {
        public string RewardId;
        public int    quantity;
    }

    public enum TypeOfScheduleReward
    {
        Online,
        Offline
    }
}