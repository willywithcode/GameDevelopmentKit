namespace GameFoundation.Scripts.Features.Inventory.LocalDatas.Controllers
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.Inventory.Blueprints;
    using GameFoundation.Scripts.Features.Inventory.LocalDatas.Models;
    using GameFoundation.Scripts.Features.Inventory.Signals;
    using GameFoundation.Scripts.Features.UserExperience.Services;
    using GameFoundation.Scripts.LocalData.Service;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using VContainer.Unity;

    public class InventoryLocalDataService : BaseLocalDataService<InventoryLocalData>, IInitializable
    {
        private readonly IAssetsManager        assetsManager;
        private readonly UserExperienceService userExperienceService;
        private readonly SignalBus             signalBus;

        public InventoryLocalDataService(
            IAssetsManager        assetsManager,
            SignalBus             signalBus,
            UserExperienceService userExperienceService
        )
        {
            this.assetsManager         = assetsManager;
            this.userExperienceService = userExperienceService;
            this.signalBus             = signalBus;
        }

        private InventoryDefault        inventoryDefault;
        private Dictionary<string, int> itemLimits = new();

        public void Initialize()
        {
            this.inventoryDefault = this.assetsManager.LoadAsset<InventoryDefault>("InventoryDefault");

            foreach (var item in this.inventoryDefault.InventoryItems)
            {
                if (item.HasLimit)
                    this.itemLimits[item.ItemId] = item.Limit;
            }

            if (this.userExperienceService.GetTimePlayed() > 0) return;

            foreach (var item in this.inventoryDefault.InventoryItems)
            {
                if (this.Data.InventoryItems.ContainsKey(item.ItemId)) continue;
                this.Data.InventoryItems.Add(item.ItemId, item.Amount);
                this.signalBus.Fire<OnInventoryValueChange>(new(item.ItemId, item.Amount));
                this.Save();
            }
        }

        public int GetItemAmount(string itemId) => this.Data.InventoryItems.GetValueOrDefault(itemId, 0);

        public int GetItemLimit(string itemId) => this.itemLimits.GetValueOrDefault(itemId, -1);

        // Returns the actual amount added (may be less than requested if a limit is hit).
        public int AddItem(string itemId, int amount)
        {
            var current   = this.Data.InventoryItems.GetValueOrDefault(itemId, 0);
            var desired   = current + amount;
            var clamped   = this.itemLimits.TryGetValue(itemId, out var limit) ? Math.Min(desired, limit) : desired;
            var actualAdded = clamped - current;

            if (actualAdded <= 0) return 0;

            if (this.Data.InventoryItems.ContainsKey(itemId))
                this.Data.InventoryItems[itemId] = clamped;
            else
                this.Data.InventoryItems.Add(itemId, clamped);

            this.Save();
            return actualAdded;
        }

        public void PayItem(string itemId, int amount)
        {
            if (this.Data.InventoryItems.ContainsKey(itemId))
            {
                this.Data.InventoryItems[itemId] -= amount;
                if (this.Data.InventoryItems[itemId] <= 0)
                {
                    this.Data.InventoryItems.Remove(itemId);
                }
            }

            this.Save();
        }
    }
}