namespace GameFoundation.Scripts.Features.Inventory.Services
{
    public interface IInventoryService
    {
        public void AddItem(string       itemId, int amount);
        public void PayItem(string       itemId, int amount);
        public int  GetItemAmount(string itemId);
        public bool CanPayItem(string itemId, int amount);
    }
}