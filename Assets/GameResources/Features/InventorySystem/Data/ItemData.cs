using GameResources.Features.EditorGridDrawled;
using UnityEngine;

namespace GameResources.Features.InventorySystem.Data
{
    [CreateAssetMenu(menuName = "Inventory/ItemData", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [field: SerializeField] public string Id { get; private set; } = default;
        [field: SerializeField] public string DisplayName { get; private set; } = default;
        [field: SerializeField] public string Description { get; private set; } = default;

        [Header("Visual")]
        [field: SerializeField] public ItemView UIPrefab { get; private set; } = default;
        [field: SerializeField] public ItemView WorldPrefab { get; private set; } = default;

        [Header("Options")]
        [SerializeField] private Wrapper<CellType>[] _grid = default;
#if UNITY_EDITOR
        [Header("Editor Helpful Grid")]
        [SerializeField] private EditorGridItem _editorGrid = new EditorGridItem();
#endif
        [field: SerializeField] public bool IsRotatable { get; private set; } = true;

        public Wrapper<CellType>[] TryGetItemSize()
        {
#if !UNITY_EDITOR
            return _grid;
#else
            return _editorGrid.GetGrid();
#endif
        }
   
#if UNITY_EDITOR
        public CellType[,] GetGridMatrix() => _editorGrid.GetGridMatrix();
#endif
   
#if UNITY_EDITOR
        
        private void OnValidate()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            _grid = _editorGrid.GetGrid();
            if (string.IsNullOrEmpty(Id) || string.IsNullOrWhiteSpace(Id))
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrEmpty(path))
                {
                    Id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                }
                else
                {
                    Id = System.Guid.NewGuid().ToString();
                }
            }
        }
#endif
    }
}