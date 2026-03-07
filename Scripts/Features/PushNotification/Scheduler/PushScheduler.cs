namespace GameFoundation.Scripts.Features.PushNotification.Scheduler
{
    using System;
    using GameFoundation.Scripts.Features.PushNotification.Config;
    using VContainer.Unity;
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
    using Unity.Notifications.Android;
#elif UNITY_IOS && UNITY_MOBILE_NOTIFICATIONS
    using Unity.Notifications.iOS;
#endif

    public class PushScheduler : IPushScheduler, IInitializable
    {
        private const string ChannelId = "game_reminders";

        private readonly PushConfig _config;

#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
        private int _dailyRewardId = -1;
        private int _comebackId    = -1;
#endif

        public PushScheduler(PushConfig config)
        {
            this._config = config;
        }

        public void Initialize()
        {
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            var channel = new AndroidNotificationChannel
            {
                Id          = ChannelId,
                Name        = "Game Reminders",
                Importance  = Importance.Default,
                Description = "Reminder notifications for daily rewards and comebacks.",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        }

        public void ScheduleDailyReward(TimeSpan delay)
        {
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            if (this._dailyRewardId >= 0)
                AndroidNotificationCenter.CancelScheduledNotification(this._dailyRewardId);

            var notification = new AndroidNotification
            {
                Title    = this._config.DailyRewardTitle,
                Text     = this._config.DailyRewardBody,
                FireTime = DateTime.Now + delay,
            };
            this._dailyRewardId = AndroidNotificationCenter.SendNotification(notification, ChannelId);
#elif UNITY_IOS && UNITY_MOBILE_NOTIFICATIONS
            iOSNotificationCenter.RemoveScheduledNotification("daily_reward");
            var trigger = new iOSNotificationTimeIntervalTrigger { TimeInterval = delay, Repeats = false };
            var notification = new iOSNotification
            {
                Identifier = "daily_reward",
                Title      = this._config.DailyRewardTitle,
                Body       = this._config.DailyRewardBody,
                Trigger    = trigger,
            };
            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        public void ScheduleComebackReminder(TimeSpan delay)
        {
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            if (this._comebackId >= 0)
                AndroidNotificationCenter.CancelScheduledNotification(this._comebackId);

            var notification = new AndroidNotification
            {
                Title    = this._config.ComebackTitle,
                Text     = this._config.ComebackBody,
                FireTime = DateTime.Now + delay,
            };
            this._comebackId = AndroidNotificationCenter.SendNotification(notification, ChannelId);
#elif UNITY_IOS && UNITY_MOBILE_NOTIFICATIONS
            iOSNotificationCenter.RemoveScheduledNotification("comeback");
            var trigger = new iOSNotificationTimeIntervalTrigger { TimeInterval = delay, Repeats = false };
            var notification = new iOSNotification
            {
                Identifier = "comeback",
                Title      = this._config.ComebackTitle,
                Body       = this._config.ComebackBody,
                Trigger    = trigger,
            };
            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        public void Cancel(int notificationId)
        {
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            AndroidNotificationCenter.CancelScheduledNotification(notificationId);
#endif
        }

        public void CancelAll()
        {
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            this._dailyRewardId = -1;
            this._comebackId    = -1;
#elif UNITY_IOS && UNITY_MOBILE_NOTIFICATIONS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }
    }
}
