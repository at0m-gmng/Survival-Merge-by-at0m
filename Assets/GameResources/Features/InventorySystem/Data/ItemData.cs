namespace GameResources.Features.InventorySystem.Data
{
    using System;
    using EditorGridDrawled;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Inventory/ItemData", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField] public BaseItem Item { get; private set; }

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
                Item.DisplayName,
                Item.Description,
                Item.UIPrefab,
                Item.WorldPrefab,
                Item.EditorGrid,
                Item.IsRotatable,
                Item.IsMergable
            )
            {
                Grid = Item.EditorGrid.GetGrid()
            };
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
    
    [Serializable]
    public struct BaseItem
    {
        public BaseItem(
            string id,
            ItemType type = ItemType.None,
            int level = default,
            string displayName = default,
            string description = default,
            ItemView uiPrefab = default,
            ItemView worldPrefab = default,
            EditorGridItem editorGrid = default,
            bool isRotatable = true,
            bool isMergable = true)
        {
            Id = id;
            Type = type;
            Level = level;
            DisplayName = displayName;
            Description = description;
            UIPrefab = uiPrefab;
            WorldPrefab = worldPrefab;
            Grid = editorGrid != null ? editorGrid.GetGrid() : default;
            EditorGrid = editorGrid != null ? editorGrid : new EditorGridItem();
            IsRotatable = isRotatable;
            IsMergable = isMergable;
        }
        
        [Header("Identity")]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }

        [Header("Visual")]
        [field: SerializeField] public ItemView UIPrefab { get; private set; }
        [field: SerializeField] public ItemView WorldPrefab { get; private set; }

        [Header("Options")]
        [field: SerializeField] public Wrapper<CellType>[] Grid { get; set; }
        [field: SerializeField] public bool IsRotatable { get; private set; }
        [field: SerializeField] public bool IsMergable { get; private set; }
        
#if UNITY_EDITOR
        [Header("Editor Helpful Grid")]
        [field: SerializeField] public EditorGridItem EditorGrid { get; private set; }
#endif

        public Wrapper<CellType>[] TryGetItemSize()
        {
#if UNITY_EDITOR
            return Grid;
#else
            return EditorGrid.GetGrid();
#endif
        }
   
#if UNITY_EDITOR
        public CellType[,] GetGridMatrix() => EditorGrid.GetGridMatrix();
#endif
        
        public BaseItem GetRotation(int rotationCount)
        {
            rotationCount = ((rotationCount % 4) + 4) % 4;
            if (rotationCount != 0)
            {
                Wrapper<CellType>[] position = Grid;
                for (int i = 0; i < rotationCount; i++)
                {
                    position = RotateOnce(position);
                }

                BaseItem rotatedItem = this;
                rotatedItem.Grid = position;
                return rotatedItem;
            }

            return this;
        }

        private Wrapper<CellType>[] RotateOnce(Wrapper<CellType>[] shape)
        {
            Wrapper<CellType>[] newShape = new Wrapper<CellType>[shape[0].Values.Length];
            for (int i = 0; i < shape[0].Values.Length; i++)
            {
                newShape[i] = new Wrapper<CellType>
                {
                    Values = new CellType[shape.Length]
                };
                for (int j = 0; j < shape.Length; j++)
                {
                    newShape[i].Values[j] = shape[shape.Length - 1 - j].Values[i];
                }
            }
            return newShape;
        }
    }
}