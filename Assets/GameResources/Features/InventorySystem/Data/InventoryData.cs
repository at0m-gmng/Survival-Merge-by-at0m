namespace GameResources.Features.InventorySystem.Data
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Inventory/InventoryData", fileName = "InventoryData")]
    public class InventoryData : ScriptableObject
    {
        public IReadOnlyList<PlacementItem> PlacedItems => _placedItems;

        [Header("Grid size")]
        [field: SerializeField] public int Columns { get; private set; } = default;
        [field: SerializeField] public int Rows { get; private set; } = default;
        [field: SerializeField] public RectTransform CellPrefab { get; private set; } = default;

        [Header("Base Items")]
        [field: SerializeField] public ItemData[] BaseItems { get; private set; } = default;

        private List<PlacementItem> _placedItems = new List<PlacementItem>();

        #region UNITY_REGION

        private void OnDisable() => ClearPlacements();

        #endregion

        #region PUBLIC_REGION
        
        public void AddOrUpdatePlacement(PlacementItem placement)
        {
            if (placement != null && !string.IsNullOrEmpty(placement.ID))
            {
                int idx = _placedItems.FindIndex(p => p.ID == placement.ID);
                if (idx >= 0)
                {
                    _placedItems[idx] = placement;
                }
                else
                {
                    _placedItems.Add(placement);
                }
            }
        }

        public bool TryRemovePlacement(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int targetIndex = _placedItems.FindIndex(p => p.ID == id);
                if (targetIndex >= 0)
                {
                    _placedItems.RemoveAt(targetIndex);
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool TryGetPlacement(string id, out PlacementItem placement)
        {
            placement = null;
            if (!string.IsNullOrEmpty(id))
            {
                placement = _placedItems.Find(p => p.ID == id);
                return placement != null;
            }
            return false;
        }

        #endregion

        #region PRIVATE_REGION

        private void ClearPlacements() => _placedItems.Clear();

        #endregion
    }
}
