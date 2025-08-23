namespace GameResources.Features.InventorySystem.Data
{
    using System;
    using EditorGridDrawled;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Inventory/ItemData", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField] public BaseItem Item { get; private set; } = null;

        private string _id = default;
        
#if UNITY_EDITOR
        
        private void OnValidate()
        {
            if (Item == null)
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

            if (Item != null)
            {
                Item.Grid = Item.EditorGrid.GetGrid();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
    
    [Serializable]
    public class BaseItem
    {
        public BaseItem(string id)
        {
            Id = id;
        }
        
        [Header("Identity")]
        [field: SerializeField] public string Id { get; private set; } = default;
        [field: SerializeField] public string DisplayName { get; private set; } = default;
        [field: SerializeField] public string Description { get; private set; } = default;

        [Header("Visual")]
        [field: SerializeField] public ItemView UIPrefab { get; private set; } = default;
        [field: SerializeField] public ItemView WorldPrefab { get; private set; } = default;

        [Header("Options")]
        [field: SerializeField] public Wrapper<CellType>[] Grid { get; set; } = default;
#if UNITY_EDITOR
        [Header("Editor Helpful Grid")]
        [field: SerializeField] public EditorGridItem EditorGrid { get; private set; } = new EditorGridItem();
#endif
        [field: SerializeField] public bool IsRotatable { get; private set; } = true;

        public Wrapper<CellType>[] TryGetItemSize()
        {
#if !UNITY_EDITOR
            return Grid;
#else
            return EditorGrid.GetGrid();
#endif
        }
   
#if UNITY_EDITOR
        public CellType[,] GetGridMatrix() => EditorGrid.GetGridMatrix();
#endif
    }
}