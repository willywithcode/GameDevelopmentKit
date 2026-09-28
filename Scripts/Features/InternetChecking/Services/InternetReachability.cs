namespace GameFoundation.Scripts.Features.InternetChecking.Services
{
    using UnityEngine;

    public interface IInternetReachability
    {
        /// <summary>
        /// Whether the device has a network route. A Wi-Fi network without internet access still
        /// reads as reachable.
        /// </summary>
        bool IsReachable { get; }
    }

    public class UnityInternetReachability : IInternetReachability
    {
        public bool IsReachable => Application.internetReachability != NetworkReachability.NotReachable;
    }
}
