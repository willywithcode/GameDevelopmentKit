namespace GameFoundation.Scripts.Features.LiveFeature.Services
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.Inventory.Services;
    using GameFoundation.Scripts.Features.LiveFeature.Blueprints;
    using GameFoundation.Scripts.Features.LiveFeature.LocalData;
    using GameFoundation.Scripts.Features.LiveFeature.Signals;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using UnityEngine;
    using VContainer.Unity;

    public class LiveService : IInitializable, IDisposable
    {
        private const string LIVE_ITEM_ID = "live";

        private readonly LiveBlueprint        liveBlueprint;
        private readonly LiveLocalDataService localData;
        private readonly IInventoryService    inventoryService;
        private readonly SignalBus            signalBus;
        private readonly IAssetsManager       assetsManager;
        private CancellationTokenSource tickCts;
        private TimeSpan                livesRefillTime;
        private TimeSpan                infinityTime;
        private bool                    isInit;

        public LiveService(
            IAssetsManager       assetsManager, 
            LiveLocalDataService localData,
            IInventoryService    inventoryService,
            SignalBus            signalBus
        )
        {
            this.localData        = localData;
            this.inventoryService = inventoryService;
            this.signalBus        = signalBus;
            this.assetsManager    = assetsManager;
            this.liveBlueprint    = this.assetsManager.LoadAsset<LiveBlueprint>("LiveBlueprint");
        }

        public void Initialize()
        {
            // First-launch guard: give initial lives if lives == 0 and no timestamp exists
            if (this.GetLives() == 0 && string.IsNullOrEmpty(this.localData.StartLivesTimeStamp))
            {
                this.SetLives(this.liveBlueprint.InitialLives);
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

        public int  GetLives()    => this.inventoryService.GetItemAmount(LIVE_ITEM_ID);
        public int GetMaxLives() => this.liveBlueprint.MaxLives;
        private int InitialLives   => this.liveBlueprint.InitialLives;
        private int LivesResetTime     => this.liveBlueprint.LivesResetTime;
        public bool HasLives()    => this.GetLives() > 0 || this.IsInfinityActive();
        public bool IsFullLives() => this.GetLives() >= this.liveBlueprint.MaxLives;

        public bool IsInfinityActive()
        {
            return !string.IsNullOrEmpty(this.localData.InfinityLivesTimeStamp)
                && this.ParseInfinityTimeStamp() > DateTime.UtcNow;
        }

        public bool SpendLife()
        {
            if (this.IsInfinityActive()) return true;

            if (this.GetLives() <= 0) return false;

            this.inventoryService.PayItem(LIVE_ITEM_ID, 1);
            this.CheckLives();
            this.FireLivesChanged();
            return true;
        }

        public void AddLives(int amount)
        {
            var currentLives = this.GetLives();
            var toAdd        = Mathf.Min(amount, this.liveBlueprint.MaxLives - currentLives);
            if (toAdd > 0) this.inventoryService.AddItem(LIVE_ITEM_ID, toAdd);

            if (this.GetLives() >= this.liveBlueprint.MaxLives)
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
            this.SetLives(this.liveBlueprint.MaxLives);
            this.FireLivesChanged();
        }

        public int GetSecondsUntilNextLife()
        {
            if (string.IsNullOrEmpty(this.localData.StartLivesTimeStamp)) return 0;

            var start   = this.ParseLiveTimeStamp();
            var elapsed = DateTime.UtcNow - start;
            var remaining = this.liveBlueprint.LivesResetTime - (elapsed.TotalSeconds % this.liveBlueprint.LivesResetTime);
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
            if (string.IsNullOrEmpty(this.localData.StartLivesTimeStamp) && this.GetLives() < this.liveBlueprint.MaxLives)
            {
                this.localData.StartLivesTimeStamp =
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void RefreshLives()
        {
            if (this.GetLives() >= this.liveBlueprint.MaxLives || this.IsInfinityActive())
            {
                this.SetLives(this.liveBlueprint.MaxLives);
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
            var heartsToAdd = (int)(this.livesRefillTime.TotalSeconds / this.liveBlueprint.LivesResetTime);
            if (heartsToAdd <= 0) return;

            var currentLives = this.GetLives();
            var newLives     = Mathf.Min(currentLives + heartsToAdd, this.liveBlueprint.MaxLives);
            var toAdd        = newLives - currentLives;
            if (toAdd > 0) this.inventoryService.AddItem(LIVE_ITEM_ID, toAdd);

            if (this.GetLives() >= this.liveBlueprint.MaxLives)
            {
                this.SetLives(this.liveBlueprint.MaxLives);
                this.localData.StartLivesTimeStamp = "";
                this.livesRefillTime = TimeSpan.Zero;
            }
            else
            {
                var remaining  = this.livesRefillTime - TimeSpan.FromSeconds(heartsToAdd * this.liveBlueprint.LivesResetTime);
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
            if (this.GetLives() < this.liveBlueprint.MaxLives
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

        // ───────── Inventory Helper ─────────

        private void SetLives(int amount)
        {
            var current = this.GetLives();
            var diff    = amount - current;
            if (diff > 0) this.inventoryService.AddItem(LIVE_ITEM_ID, diff);
            else if (diff < 0) this.inventoryService.PayItem(LIVE_ITEM_ID, -diff);
        }

        // ───────── Signal Firing ─────────

        private void FireLivesChanged()
        {
            this.signalBus.Fire(new OnLivesChanged
            {
                CurrentLives = this.GetLives(),
                MaxLives     = this.liveBlueprint.MaxLives,
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
