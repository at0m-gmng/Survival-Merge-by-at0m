namespace GameResources.Features.MergeData
{
    using System;
    using InventorySystem;
    using InventorySystem.Data;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Inventory/MergeData", fileName = "MergeData")]
    public class MergeData : ScriptableObject
    {
        [field: SerializeField] public MergeRule[] Rules { get; private set; }
    }
    
    [Serializable]
    public class MergeRule
    {
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public ItemData ResultItem { get; private set; }
    }
}