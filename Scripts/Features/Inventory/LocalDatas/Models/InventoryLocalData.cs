namespace GameFoundation.Scripts.Features.Inventory.LocalDatas.Models
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.LocalData.Interfaces;

    public class InventoryLocalData : ILocalData
    {
        public Dictionary<string, int> InventoryItems { get; set; } = new();
        public string                  GetKey()       => this.GetType().ToString();

        public void Reset()
        {
            this.InventoryItems = new();
        }
    }
}