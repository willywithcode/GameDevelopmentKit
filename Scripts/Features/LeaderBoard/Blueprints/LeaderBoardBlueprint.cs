namespace GameFoundation.Scripts.Features.LeaderBoard.Blueprints
{
    using System;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Attributes;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Services;
    using UnityEngine;

    [CreateAssetMenu(fileName = "LeaderBoardBlueprint", menuName = "GameFoundation/Features/LeaderBoard/Blueprint")]
    public class LeaderBoardBlueprint : ScriptableObject
    {
        [Header("Bot Count")]
        [Tooltip("Number of fake bots on the leaderboard")]
        public int BotCount = 19;

        [Header("Initial Score Range")]
        [Tooltip("Minimum score for a newly generated bot")]
        public int InitialScoreMin = 50;

        [Tooltip("Maximum score for a newly generated bot")]
        public int InitialScoreMax = 5000;

        [Header("Online Score Gain (while app is open)")]
        [Tooltip("How often bots gain score while user is online (seconds)")]
        public float OnlineIntervalSeconds = 60f;

        [Tooltip("Min score a bot gains per online tick")]
        public int OnlineScoreGainMin = 5;

        [Tooltip("Max score a bot gains per online tick")]
        public int OnlineScoreGainMax = 30;

        [Header("Offline Score Gain (while app is closed)")]
        [Tooltip("How often bots gain score while user is offline (seconds)")]
        public float OfflineIntervalSeconds = 60f;

        [Tooltip("Min score a bot gains per offline tick")]
        public int OfflineScoreGainMin = 3;

        [Tooltip("Max score a bot gains per offline tick")]
        public int OfflineScoreGainMax = 20;

        [Tooltip("Maximum offline time counted (hours). Prevents extreme gains after long absence.")]
        public float MaxOfflineHours = 24f;

        [Header("Bot Names")]
        public string[] BotNames =
        {
            "AquaKing", "BubbleMaster", "WaveRider", "SplashBot",
            "TidalForce", "DeepDiver", "OceanStorm", "CoralReef",
            "SeaBreeze", "RipCurrent", "Tsunami", "Droplet",
            "MistWalker", "FrostDrop", "RainDancer", "PuddleJump",
            "StreamFlow", "IceCap", "DewPoint", "Typhoon"
        };
    }

    [SOBlueprintAttributes(nameof(LeaderBoardBlueprint))]
    public class LeaderBoardBlueprintService : BaseSOBlueprintService<LeaderBoardBlueprint>
    {
        public LeaderBoardBlueprintService(IAssetsManager assetsManager) : base(assetsManager) { }
    }
}
