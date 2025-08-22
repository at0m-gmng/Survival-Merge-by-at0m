using UnityEngine;

namespace GameResources.Features.InventorySystem.Data
{
    [CreateAssetMenu(menuName = "Inventory/InventoryData", fileName = "InventoryData")]
    public class InventoryData : ScriptableObject
    {
        [Header("Grid size")]
        [field: SerializeField] public int Columns { get; private set; } = default;
        [field: SerializeField] public int Rows { get; private set; } = default;
        [field: SerializeField] public RectTransform CellPrefab { get; private set; } = default;
        
        [Header("Base Items")]
        [field: SerializeField] public  ItemData[] BaseItems { get; private set; } = default;
    }
}