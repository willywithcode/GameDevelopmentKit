namespace GameFoundation.Scripts.Features.Inventory.Signals
{
    public class OnInventoryValueChange
    {
        public string ItemId { get; }

        public OnInventoryValueChange(string itemId)
        {
            this.ItemId = itemId;
        }
    }
}