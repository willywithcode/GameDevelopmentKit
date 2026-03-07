namespace GameFoundation.Scripts.Features.PushNotification.DeepLink
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Features.PushNotification.Models;
    using GameFoundation.Scripts.Features.PushNotification.Services;
    using VContainer.Unity;

    public class PushDeepLinkHandler : IPushDeepLinkHandler, IInitializable
    {
        private readonly IPushService                       pushService;
        private readonly Dictionary<string, Action<string>> handlers = new();

        public PushDeepLinkHandler(IPushService pushService) { this.pushService = pushService; }

        public void Initialize()
        {
            this.pushService.OnNotificationClicked += this.OnNotificationClicked;
        }

        public void Register(string key, Action<string> handler) { this.handlers[key] = handler; }

        public void Handle(string deepLink)
        {
            if (string.IsNullOrEmpty(deepLink)) return;

            var uri = new Uri(deepLink);
            var key = uri.Host;

            if (this.handlers.TryGetValue(key, out var handler)) handler(deepLink);
            // else
            //     Logger.Log(Channel.UI, Priority.Warning, $"No deeplink handler registered for: {key}");
        }

        private void OnNotificationClicked(PushPayload payload)
        {
            this.Handle(payload.DeepLink);
        }
    }
}