namespace GameFoundation.Scripts.Features.Profile.Blueprints
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "LocalDataBlueprint", menuName = "GameFoundation/Features/Profile/LocalDataBlueprint")]
    public class ProfileBlueprint : ScriptableObject
    {
        [SerializeField] private string defaultDisplayName = "Player";
        [SerializeField, Min(1)] private int maxDisplayNameLength = 16;
        [SerializeField, Min(0)] private int defaultAvatarIndex = 0;

        public string DefaultDisplayName => this.defaultDisplayName;
        public int    MaxDisplayNameLength => this.maxDisplayNameLength;
        public int    DefaultAvatarIndex => this.defaultAvatarIndex;
    }
}
