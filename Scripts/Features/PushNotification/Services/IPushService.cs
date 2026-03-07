namespace GameFoundation.Scripts.Features.PushNotification.Services
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.PushNotification.Models;

    public interface IPushService
    {
        bool IsPermissionGranted { get; }
        void Initialize();
        UniTask<bool> RequestPermissionAsync();
        event Action<PushPayload> OnNotificationClicked;
        event Action<PushPayload> OnNotificationReceived;
    }
}
