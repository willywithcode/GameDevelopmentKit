namespace GameFoundation.Scripts.Features.Inventory.Services
{
    using GameFoundation.Scripts.Features.Inventory.LocalDatas.Controllers;
    using GameFoundation.Scripts.Features.Inventory.Signals;
    using GameFoundation.Scripts.Patterns.SignalBus;

    public class InventoryService : IInventoryService
    {
        #region Inject

        private readonly InventoryLocalDataService inventoryLocalDataService;
        private readonly SignalBus                 signalBus;

        public InventoryService(
            InventoryLocalDataService inventoryLocalDataService,
            SignalBus                 signalBus
        )
        {
            this.inventoryLocalDataService = inventoryLocalDataService;
            this.signalBus                 = signalBus;
        }

        #endregion

        public void AddItem(string itemId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            this.inventoryLocalDataService.AddItem(itemId, amount);
            this.signalBus.Fire<OnInventoryValueChange>(new(itemId));
        }

        public void PayItem(string itemId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            this.inventoryLocalDataService.PayItem(itemId, amount);
            this.signalBus.Fire<OnInventoryValueChange>(new(itemId));
        }

        public int GetItemAmount(string itemId) => this.inventoryLocalDataService.GetItemAmount(itemId);

        public bool CanPayItem(string itemId, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            var itemAmount = this.inventoryLocalDataService.GetItemAmount(itemId);
            return itemAmount >= amount;
        }
    }
}