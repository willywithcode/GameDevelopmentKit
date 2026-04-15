namespace GameFoundation.Scripts.Features.Inventory.Services
{
    using GameFoundation.Scripts.Features.Inventory.LocalDatas.Controllers;
    using GameFoundation.Scripts.Features.Inventory.Signals;
    using MessagePipe;

    public class InventoryService : IInventoryService
    {
        #region Inject

        private readonly InventoryLocalDataService         inventoryLocalDataService;
        private readonly IPublisher<OnInventoryValueChange> inventoryValueChangePublisher;

        public InventoryService(
            InventoryLocalDataService         inventoryLocalDataService,
            IPublisher<OnInventoryValueChange> inventoryValueChangePublisher
        )
        {
            this.inventoryLocalDataService     = inventoryLocalDataService;
            this.inventoryValueChangePublisher = inventoryValueChangePublisher;
        }

        #endregion

        public void AddItem(string itemId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var actualAdded = this.inventoryLocalDataService.AddItem(itemId, amount);
            if (actualAdded > 0)
                this.inventoryValueChangePublisher.Publish(new OnInventoryValueChange(itemId, actualAdded));
        }

        public void PayItem(string itemId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            this.inventoryLocalDataService.PayItem(itemId, amount);
            this.inventoryValueChangePublisher.Publish(new OnInventoryValueChange(itemId, -amount));
        }

        public int GetItemAmount(string itemId) => this.inventoryLocalDataService.GetItemAmount(itemId);

        public int GetItemLimit(string itemId) => this.inventoryLocalDataService.GetItemLimit(itemId);

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
