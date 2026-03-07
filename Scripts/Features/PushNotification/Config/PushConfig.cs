namespace GameFoundation.Scripts.Features.PushNotification.Config
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "GameFoundation/Push Config")]
    public class PushConfig : ScriptableObject
    {
        [SerializeField] private string oneSignalAppId;
        [SerializeField] private string dailyRewardTitle;
        [SerializeField] private string dailyRewardBody;
        [SerializeField] private string comebackTitle;
        [SerializeField] private string comebackBody;

        public string OneSignalAppId  => this.oneSignalAppId;
        public string DailyRewardTitle => this.dailyRewardTitle;
        public string DailyRewardBody  => this.dailyRewardBody;
        public string ComebackTitle    => this.comebackTitle;
        public string ComebackBody     => this.comebackBody;
    }
}
