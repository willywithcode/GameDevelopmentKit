namespace GameFoundation.Scripts.Features.InternetChecking.Services
{
    using UnityEngine;

    public interface INetworkSettingsOpener
    {
        /// <summary>Opens the system screen where the player can turn the network on. Returns whether one opened.</summary>
        bool Open();
    }

    public class AndroidNetworkSettingsOpener : INetworkSettingsOpener
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private const int InternetPanelMinSdk = 29;

        // Settings.Panel.ACTION_INTERNET_CONNECTIVITY: an overlay over the game, API 29+.
        private const string InternetPanelAction = "android.settings.panel.action.INTERNET_CONNECTIVITY";

        // Settings.ACTION_WIFI_SETTINGS, then Settings.ACTION_WIRELESS_SETTINGS: full screens that
        // some vendor ROMs keep when they drop the panel.
        private static readonly string[] FallbackActions = { "android.settings.WIFI_SETTINGS", "android.settings.WIRELESS_SETTINGS" };

        public bool Open()
        {
            if (GetSdkInt() >= InternetPanelMinSdk && TryStartActivity(InternetPanelAction)) return true;

            foreach (var action in FallbackActions)
                if (TryStartActivity(action))
                    return true;

            Debug.LogError("[InternetChecking] No network settings screen could be opened on this device.");
            return false;
        }

        private static int GetSdkInt()
        {
            using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
            return buildVersion.GetStatic<int>("SDK_INT");
        }

        private static bool TryStartActivity(string action)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var intent      = new AndroidJavaObject("android.content.Intent", action);
                activity.Call("startActivity", intent);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InternetChecking] Could not open {action}: {e.Message}");
                return false;
            }
        }
#else
        public bool Open()
        {
            Debug.Log("[InternetChecking] No internet. The network settings open only on an Android device.");
            return false;
        }
#endif
    }
}
