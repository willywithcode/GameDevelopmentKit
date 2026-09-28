namespace GameFoundation.Scripts.Features.InternetChecking.Services
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Events;

    public enum ConnectivityChange
    {
        None,
        Lost,
        Restored,
    }

    public class InternetCheckingService
    {
        public const int MaxCountToFirmlyCheck = 3;

        private static readonly TimeSpan SampleInterval = TimeSpan.FromSeconds(1);

        private readonly IInternetReachability reachability;

        private int count;

        public InternetCheckingService(IInternetReachability reachability) { this.reachability = reachability; }

        /// <summary>
        /// Reads as offline regardless of the device state, so QA can rehearse the offline flow
        /// on a connected device.
        /// </summary>
        public bool ForceOffline { get; set; }

        /// <summary>The instantaneous reading, without the consecutive-sample debounce.</summary>
        public bool IsOnline => !this.ForceOffline && this.reachability.IsReachable;

        /// <summary>True from the sample that reported <see cref="ConnectivityChange.Lost"/> until the one that reported <see cref="ConnectivityChange.Restored"/>.</summary>
        public bool IsFirmlyOffline { get; private set; }

        /// <summary>
        /// One polling step. A single failed reading is not trusted: the connection counts as lost
        /// only after <see cref="MaxCountToFirmlyCheck"/> consecutive failures, and any online
        /// reading before that resets the count.
        /// </summary>
        public ConnectivityChange Sample()
        {
            if (this.IsOnline)
            {
                this.count = 0;
                if (!this.IsFirmlyOffline) return ConnectivityChange.None;

                this.IsFirmlyOffline = false;
                return ConnectivityChange.Restored;
            }

            if (this.IsFirmlyOffline) return ConnectivityChange.None;
            if (++this.count < MaxCountToFirmlyCheck) return ConnectivityChange.None;

            this.count           = 0;
            this.IsFirmlyOffline = true;
            return ConnectivityChange.Lost;
        }

        /// <summary>
        /// Samples once a second until <paramref name="cancellationToken"/> is cancelled. Runs one
        /// loop per service: every loop shares the same consecutive-failure count.
        /// </summary>
        public async UniTask StartCheckingInternetAsync(UnityAction onNoInternet, UnityAction onHasInternet, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                switch (this.Sample())
                {
                    case ConnectivityChange.Lost:
                        onNoInternet?.Invoke();
                        break;
                    case ConnectivityChange.Restored:
                        onHasInternet?.Invoke();
                        break;
                }

                // Real time, so a game that sets timeScale to 0 still notices a lost connection.
                if (await UniTask.Delay(SampleInterval, DelayType.Realtime, cancellationToken: cancellationToken).SuppressCancellationThrow()) return;
            }
        }
    }
}
