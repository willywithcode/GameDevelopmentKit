namespace GameFoundation.Scripts.Features.PushNotification.Services
{
    #if ONE_SIGNAL
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.PushNotification.Config;
    using GameFoundation.Scripts.Features.PushNotification.Models;
    using VContainer.Unity;
    using OneSignalSDK;
    using OneSignalSDK.Notifications;

    public class OneSignalPushService : IPushService, IInitializable, IDisposable
    {
        private readonly PushConfig _config;

        public event Action<PushPayload> OnNotificationClicked;
        public event Action<PushPayload> OnNotificationReceived;

        public bool IsPermissionGranted => OneSignal.Notifications.Permission;

        public OneSignalPushService(PushConfig config)
        {
            this._config = config;
        }

        public void Initialize()
        {
            OneSignal.Initialize(this._config.OneSignalAppId);
            OneSignal.Notifications.Clicked             += this.HandleClicked;
            OneSignal.Notifications.ForegroundWillDisplay += this.HandleForeground;
        }

        public async UniTask<bool> RequestPermissionAsync()
        {
            return await OneSignal.Notifications.RequestPermissionAsync(true);
        }

        public void Dispose()
        {
            OneSignal.Notifications.Clicked             -= this.HandleClicked;
            OneSignal.Notifications.ForegroundWillDisplay -= this.HandleForeground;
        }
        private void HandleClicked(object sender, NotificationClickEventArgs e)
        {
            var payload = this.BuildPayload(
                e.Notification.Title,
                e.Notification.Body,
                e.Notification.AdditionalData
            );
            this.OnNotificationClicked?.Invoke(payload);
        }

        private void HandleForeground(object sender, NotificationWillDisplayEventArgs e)
        {
            e.Notification.Display();
            var payload = this.BuildPayload(
                e.Notification.Title,
                e.Notification.Body,
                e.Notification.AdditionalData
            );
            this.OnNotificationReceived?.Invoke(payload);
        }

        private PushPayload BuildPayload(string title, string body, IDictionary<string, object> additionalData)
        {
            var data     = new Dictionary<string, string>();
            var deepLink = string.Empty;

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    var value = kvp.Value?.ToString() ?? string.Empty;
                    data[kvp.Key] = value;
                }

                data.TryGetValue("deeplink", out deepLink);
            }

            return new()
            {
                Title          = title,
                Body           = body,
                DeepLink       = deepLink,
                AdditionalData = data,
            };
        }
    }
#endif
}
