namespace GameFoundation.Scripts.Features.PushNotification.Scheduler
{
    using System;

    public interface IPushScheduler
    {
        void ScheduleDailyReward(TimeSpan delay);
        void ScheduleComebackReminder(TimeSpan delay);
        void Cancel(int notificationId);
        void CancelAll();
    }
}
