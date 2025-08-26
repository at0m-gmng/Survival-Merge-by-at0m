namespace GameResources.Features.InventorySystem.Data
{
    using System;
    using System.Collections.Generic;
    using EditorGridDrawled;
    using Matrix;
    using UnityEngine;

    [Serializable]
    public struct BaseItem
    {
        public BaseItem(
            string id,
            ItemType type = ItemType.None,
            int level = default,
            ItemView uiPrefab = default,
            ItemView worldPrefab = default,
            EditorGridItem editorGrid = default,
            bool isRotatable = true,
            bool isMergable = true)
        {
            Id = id;
            Type = type;
            Level = level;
            UIPrefab = uiPrefab;
            WorldPrefab = worldPrefab;
            Grid = editorGrid != null ? Matrix.Clone(editorGrid.GetGrid()) : new Matrix { Rows = new List<ColumnList>() };
            EditorGrid = editorGrid != null ? editorGrid : new EditorGridItem();
            IsRotatable = isRotatable;
            IsMergable = isMergable;
        }

        [field: SerializeField] public Matrix Grid { get; set; }

        [Header("Identity")]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public int Level { get; private set; }

        [Header("Visual")]
        [field: SerializeField] public ItemView UIPrefab { get; private set; }
        [field: SerializeField] public ItemView WorldPrefab { get; private set; }

        [Header("Options")]
        [field: SerializeField] public bool IsRotatable { get; private set; }
        [field: SerializeField] public bool IsMergable { get; private set; }
        
#if UNITY_EDITOR
        [Header("Editor Helpful Grid")]
        [field: SerializeField] public EditorGridItem EditorGrid { get; private set; }
#endif

        public Matrix TryGetItemSize() => Grid;
        
        public BaseItem GetRotation(int rotationCount)
        {
            rotationCount = ((rotationCount % 4) + 4) % 4;
            Matrix position = Matrix.Clone(Grid);
            for (int i = 0; i < rotationCount; i++)
            {
                position = Matrix.Clone(RotateOnce(position));
            }
            BaseItem rotatedItem = this;
            rotatedItem.Grid = Matrix.Clone(position);
            return rotatedItem;
        }

        private Matrix RotateOnce(Matrix shape)
        {
            if (shape.Rows != null && shape.Rows.Count != 0)
            {
                Matrix result = new Matrix { Rows = new List<ColumnList>(shape.Rows[0].Columns.Count) };

                for (int i = 0; i < shape.Rows[0].Columns.Count; i++)
                {
                    ColumnList newRow = new ColumnList { Columns = new List<int>(shape.Rows.Count) };
                    for (int j = 0; j < shape.Rows.Count; j++)
                    {
                        int value = shape.Rows[shape.Rows.Count - 1 - j].Columns[i];
                        newRow.Columns.Add(value);
                    }
                    result.Rows.Add(newRow);
                }
                return result;
            }
            return new Matrix { Rows = new List<ColumnList>() };
        }
    }
}