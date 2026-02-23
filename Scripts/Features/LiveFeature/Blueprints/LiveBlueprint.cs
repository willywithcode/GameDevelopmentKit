namespace GameFoundation.Scripts.Features.LiveFeature.Blueprints
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "LiveBlueprint", menuName = "HyperCasual/LiveBlueprint")]
    public class LiveBlueprint : ScriptableObject
    {
        [Header("Lives Settings")]
        public int MaxLives = 5;
        public int InitialLives = 5;

        [Header("Refill Settings")]
        [Tooltip("Time in seconds to refill one life")]
        public int LivesResetTime = 1800; // 30 minutes
    }
}
