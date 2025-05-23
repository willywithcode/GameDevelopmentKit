namespace GameFoundation.Scripts.Features.Inventory.LocalDatas.Controllers
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.Inventory.Blueprints;
    using GameFoundation.Scripts.Features.Inventory.LocalDatas.Models;
    using GameFoundation.Scripts.Features.Inventory.Signals;
    using GameFoundation.Scripts.LocalData.Service;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using VContainer.Unity;

    public class InventoryLocalDataService : BaseLocalDataService<InventoryLocalData>, IInitializable
    {
        private readonly IAssetsManager assetsManager;
        private readonly SignalBus      signalBus;

        public InventoryLocalDataService(
            IAssetsManager assetsManager,
            SignalBus      signalBus
        )
        {
            this.assetsManager = assetsManager;
            this.signalBus     = signalBus;
        }

        private InventoryDefault inventoryDefault;

        public void Initialize()
        {
            this.inventoryDefault = this.assetsManager.LoadAsset<InventoryDefault>("InventoryDefault");
            foreach (var item in this.inventoryDefault.InventoryItems)
            {
                if (this.Data.InventoryItems.ContainsKey(item.ItemId)) continue;
                this.Data.InventoryItems.Add(item.ItemId, item.Amount);
                this.signalBus.Fire<OnInventoryValueChange>(new(item.ItemId));
            }
        }

        public int GetItemAmount(string itemId)
        {
            return this.Data.InventoryItems.GetValueOrDefault(itemId, 0);
        }

        public void AddItem(string itemId, int amount)
        {
            if (this.Data.InventoryItems.ContainsKey(itemId))
            {
                this.Data.InventoryItems[itemId] += amount;
            }
            else
            {
                this.Data.InventoryItems.Add(itemId, amount);
            }

            this.Save();
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