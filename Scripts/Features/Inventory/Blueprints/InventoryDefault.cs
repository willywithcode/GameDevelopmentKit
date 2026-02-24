namespace GameFoundation.Scripts.Features.Inventory.Blueprints
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = "InventoryDefault", menuName = "HyperCasual/Inventory/InventoryDefault")]
    public class InventoryDefault : ScriptableObject
    {
        public InventoryItem[] InventoryItems;
    }

    [Serializable]
    public class InventoryItem
    {
        public string ItemId;
        public int    Amount;
        public bool HasLimit;
        [ShowIf(nameof(HasLimit))]
        public int Limit;
    }
}