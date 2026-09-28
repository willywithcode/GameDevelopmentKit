namespace GameFoundation.Scripts.Features.InternetChecking.Services
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VContainer.Unity;

    /// <summary>
    /// Keeps the game unplayable without internet by sending the player to the system network
    /// settings, with no in-game UI. The settings open when the connection is firmly lost, and
    /// again every time the player comes back to the game still offline. Enabled by the
    /// <c>INTERNET_REQUIRED</c> define.
    /// </summary>
    public class InternetRequiredService : IInitializable, IDisposable
    {
        // Android needs a moment after the settings close before reachability reflects the change.
        private static readonly TimeSpan ReopenDelay = TimeSpan.FromSeconds(1);

        private readonly InternetCheckingService internetCheckingService;
        private readonly INetworkSettingsOpener  networkSettingsOpener;
        private readonly CancellationTokenSource lifetimeCts = new();

        private CancellationTokenSource reopenCts;

        public InternetRequiredService(InternetCheckingService internetCheckingService, INetworkSettingsOpener networkSettingsOpener)
        {
            this.internetCheckingService = internetCheckingService;
            this.networkSettingsOpener   = networkSettingsOpener;
        }

        public bool IsBlocking { get; private set; }

        public void Initialize()
        {
            Application.focusChanged += this.OnFocusChanged;
            this.internetCheckingService.StartCheckingInternetAsync(this.HandleInternetLost, this.HandleInternetRestored, this.lifetimeCts.Token).Forget();
        }

        public void Dispose()
        {
            Application.focusChanged -= this.OnFocusChanged;
            this.CancelReopen();
            this.lifetimeCts.Cancel();
            this.lifetimeCts.Dispose();
        }

        internal void HandleInternetLost()
        {
            this.IsBlocking = true;
            this.networkSettingsOpener.Open();
        }

        internal void HandleInternetRestored() { this.IsBlocking = false; }

        /// <summary>
        /// Uses the instantaneous reading rather than the debounced one: the player has just come
        /// back from the settings, so a connection they turned on must not be met with the
        /// settings again.
        /// </summary>
        internal void ReopenIfStillOffline()
        {
            if (!this.IsBlocking || this.internetCheckingService.IsOnline) return;

            this.networkSettingsOpener.Open();
        }

        private void OnFocusChanged(bool hasFocus)
        {
            this.CancelReopen();
            if (!hasFocus || !this.IsBlocking) return;

            this.reopenCts = CancellationTokenSource.CreateLinkedTokenSource(this.lifetimeCts.Token);
            this.ReopenAfterDelayAsync(this.reopenCts.Token).Forget();
        }

        private async UniTaskVoid ReopenAfterDelayAsync(CancellationToken cancellationToken)
        {
            if (await UniTask.Delay(ReopenDelay, DelayType.Realtime, cancellationToken: cancellationToken).SuppressCancellationThrow()) return;

            this.ReopenIfStillOffline();
        }

        private void CancelReopen()
        {
            this.reopenCts?.Cancel();
            this.reopenCts?.Dispose();
            this.reopenCts = null;
        }
    }
}
