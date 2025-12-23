namespace GameFoundation.Scripts.Features.Inventory.Signals
{
    public class OnInventoryValueChange
    {
        public string ItemId       { get; }
        public int    ChangedValue { get; }

        public OnInventoryValueChange(string itemId, int changedValue)
        {
            this.ItemId       = itemId;
            this.ChangedValue = changedValue;
        }
    }
}