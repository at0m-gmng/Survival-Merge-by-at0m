namespace GameResources.Features.InventorySystem.Data
{
    using System;
    using Matrix;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Inventory/ItemData", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField] public BaseItem Item { get; private set; }
        [field: SerializeField] public ItemDataView ItemDataView { get; private set; } = new ItemDataView();

        private string _id = default;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Item.Id == default)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrEmpty(path))
                {
                    _id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                }
                else
                {
                    _id = Guid.NewGuid().ToString();
                }
                Item = new BaseItem(_id);
            }

            Item = new BaseItem(
                Item.Id,
                Item.Type,
                Item.Level,
                Item.UIPrefab,
                Item.WorldPrefab,
                Item.EditorGrid,
                Item.IsRotatable,
                Item.IsMergable
            )
            {
                Grid = Item.EditorGrid != null ? Matrix.Clone(Item.EditorGrid.GetGrid()) : Item.Grid
            };
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
