namespace GameFoundation.Scripts.Features.PushNotification.DeepLink
{
    using System;

    public interface IPushDeepLinkHandler
    {
        void Register(string key, Action<string> handler);
        void Handle(string deepLink);
    }
}
