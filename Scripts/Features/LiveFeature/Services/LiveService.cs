namespace GameFoundation.Scripts.Features.LiveFeature.Services
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.LiveFeature.LocalData;
    using GameFoundation.Scripts.Features.LiveFeature.Signals;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using UnityEngine;
    using VContainer.Unity;

    public class LiveService : IInitializable, IDisposable
    {
        private const int MAX_LIVES        = 5;
        private const int LIVES_RESET_TIME = 1800; // 30 minutes in seconds

        private readonly LiveLocalDataService localData;
        private readonly SignalBus             signalBus;

        private CancellationTokenSource tickCts;
        private TimeSpan                livesRefillTime;
        private TimeSpan                infinityTime;
        private bool                    isInit;

        public LiveService(LiveLocalDataService localData, SignalBus signalBus)
        {
            this.localData = localData;
            this.signalBus = signalBus;
        }

        public void Initialize()
        {
            // First-launch guard: give max lives if lives == 0 and no timestamp exists
            if (this.localData.Lives == 0 && string.IsNullOrEmpty(this.localData.StartLivesTimeStamp))
            {
                this.localData.Lives = MAX_LIVES;
            }

            this.CheckRewindTime();
            this.RefreshLives();
            this.RefreshInfinityLives();
            this.StartTickLoop();
            this.isInit = true;
        }

        public void Dispose()
        {
            this.tickCts?.Cancel();
            this.tickCts?.Dispose();
            this.tickCts = null;
        }

        // ───────── Public API ─────────

        public int  GetLives()    => this.localData.Lives;
        public int  GetMaxLives() => MAX_LIVES;
        public bool HasLives()    => this.localData.Lives > 0 || this.IsInfinityActive();
        public bool IsFullLives() => this.localData.Lives >= MAX_LIVES;

        public bool IsInfinityActive()
        {
            return !string.IsNullOrEmpty(this.localData.InfinityLivesTimeStamp)
                && this.ParseInfinityTimeStamp() > DateTime.UtcNow;
        }

        public bool SpendLife()
        {
            if (this.IsInfinityActive()) return true;

            if (this.localData.Lives <= 0) return false;

            this.localData.Lives = Mathf.Max(this.localData.Lives - 1, 0);
            this.CheckLives();
            this.FireLivesChanged();
            return true;
        }

        public void AddLives(int amount)
        {
            this.localData.Lives = Mathf.Min(this.localData.Lives + amount, MAX_LIVES);

            if (this.localData.Lives >= MAX_LIVES)
            {
                this.localData.StartLivesTimeStamp = "";
            }

            this.FireLivesChanged();
        }

        public void AddInfinityTime(float hours)
        {
            if (this.IsInfinityActive())
            {
                var endTime = this.ParseInfinityTimeStamp();
                this.localData.InfinityLivesTimeStamp =
                    endTime.AddHours(hours).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                this.localData.InfinityStartTimeStamp =
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                this.localData.InfinityLivesTimeStamp =
                    DateTime.UtcNow.AddHours(hours).ToString(CultureInfo.InvariantCulture);
            }

            this.infinityTime = this.ParseInfinityTimeStamp() - DateTime.UtcNow;
            this.localData.Lives = MAX_LIVES;
            this.FireLivesChanged();
        }

        public int GetSecondsUntilNextLife()
        {
            if (string.IsNullOrEmpty(this.localData.StartLivesTimeStamp)) return 0;

            var start   = this.ParseLiveTimeStamp();
            var elapsed = DateTime.UtcNow - start;
            var remaining = LIVES_RESET_TIME - (elapsed.TotalSeconds % LIVES_RESET_TIME);
            return remaining < 0 ? 0 : (int)remaining;
        }

        public int GetInfinitySecondsLeft()
        {
            if (!this.IsInfinityActive()) return 0;
            return Mathf.Max(0, (int)(this.ParseInfinityTimeStamp() - DateTime.UtcNow).TotalSeconds);
        }

        // ───────── Internal Logic ─────────

        private void CheckLives()
        {
            if (string.IsNullOrEmpty(this.localData.StartLivesTimeStamp) && this.localData.Lives < MAX_LIVES)
            {
                this.localData.StartLivesTimeStamp =
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void RefreshLives()
        {
            if (this.localData.Lives >= MAX_LIVES || this.IsInfinityActive())
            {
                this.localData.Lives = MAX_LIVES;
                this.localData.StartLivesTimeStamp = "";
                return;
            }

            if (string.IsNullOrEmpty(this.localData.StartLivesTimeStamp))
            {
                this.localData.StartLivesTimeStamp =
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void RefreshInfinityLives()
        {
            if (!string.IsNullOrEmpty(this.localData.InfinityLivesTimeStamp))
            {
                this.infinityTime = this.ParseInfinityTimeStamp() - DateTime.UtcNow;
            }
        }

        private void CheckRewindTime()
        {
            if (string.IsNullOrEmpty(this.localData.InfinityStartTimeStamp)
                || string.IsNullOrEmpty(this.localData.InfinityLivesTimeStamp))
                return;

            if (!DateTime.TryParse(this.localData.InfinityStartTimeStamp,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime))
                return;

            if (DateTime.UtcNow < startTime)
            {
                Debug.Log("[LiveService] Clock rewind detected");
                var timeDifference = startTime - DateTime.UtcNow;
                this.localData.InfinityStartTimeStamp =
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                var endTime = this.ParseInfinityTimeStamp();
                this.localData.InfinityLivesTimeStamp =
                    endTime.Subtract(timeDifference).ToString(CultureInfo.InvariantCulture);
            }
        }

        private void CheckRefill()
        {
            var heartsToAdd = (int)(this.livesRefillTime.TotalSeconds / LIVES_RESET_TIME);
            if (heartsToAdd <= 0) return;

            this.localData.Lives = Mathf.Min(this.localData.Lives + heartsToAdd, MAX_LIVES);

            if (this.localData.Lives >= MAX_LIVES)
            {
                this.localData.Lives = MAX_LIVES;
                this.localData.StartLivesTimeStamp = "";
                this.livesRefillTime = TimeSpan.Zero;
            }
            else
            {
                var remaining  = this.livesRefillTime - TimeSpan.FromSeconds(heartsToAdd * LIVES_RESET_TIME);
                var nextStart  = DateTime.UtcNow - remaining;
                this.localData.StartLivesTimeStamp =
                    nextStart.ToString(CultureInfo.InvariantCulture);
            }

            this.FireLivesChanged();
        }

        private void CheckInfinityTime()
        {
            if (this.infinityTime.TotalSeconds <= 0
                && !string.IsNullOrEmpty(this.localData.InfinityLivesTimeStamp))
            {
                this.localData.InfinityLivesTimeStamp = "";
                this.infinityTime = TimeSpan.Zero;
                this.FireLivesChanged();
            }
        }

        // ───────── Tick Loop ─────────

        private void StartTickLoop()
        {
            this.tickCts?.Cancel();
            this.tickCts = new CancellationTokenSource();
            this.TickLoopAsync(this.tickCts.Token).Forget();
        }

        private async UniTaskVoid TickLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                this.UpdateTime();
                await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true,
                    cancellationToken: ct);
            }
        }

        private void UpdateTime()
        {
            if (this.localData.Lives < MAX_LIVES
                && !string.IsNullOrEmpty(this.localData.StartLivesTimeStamp))
            {
                this.livesRefillTime = DateTime.UtcNow - this.ParseLiveTimeStamp();
                this.CheckRefill();
            }

            if (!string.IsNullOrEmpty(this.localData.InfinityLivesTimeStamp))
            {
                this.infinityTime = this.ParseInfinityTimeStamp() - DateTime.UtcNow;
                this.CheckInfinityTime();
            }

            this.FireTimerTick();
        }

        // ───────── Timestamp Parsing ─────────

        private DateTime ParseLiveTimeStamp()
        {
            if (!DateTime.TryParse(this.localData.StartLivesTimeStamp,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            {
                start = DateTime.UtcNow;
                this.localData.StartLivesTimeStamp =
                    start.ToString(CultureInfo.InvariantCulture);
            }

            return start;
        }

        private DateTime ParseInfinityTimeStamp()
        {
            if (!DateTime.TryParse(this.localData.InfinityLivesTimeStamp,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
            {
                end = DateTime.UtcNow;
                this.localData.InfinityLivesTimeStamp =
                    end.ToString(CultureInfo.InvariantCulture);
            }

            return end;
        }

        // ───────── Signal Firing ─────────

        private void FireLivesChanged()
        {
            this.signalBus.Fire(new OnLivesChanged
            {
                CurrentLives = this.localData.Lives,
                MaxLives     = MAX_LIVES,
                IsInfinity   = this.IsInfinityActive(),
            });
        }

        private void FireTimerTick()
        {
            this.signalBus.Fire(new OnLivesTimerTick
            {
                SecondsUntilNextLife = this.GetSecondsUntilNextLife(),
                IsInfinityActive    = this.IsInfinityActive(),
                InfinitySecondsLeft = this.GetInfinitySecondsLeft(),
            });
        }
    }
}
