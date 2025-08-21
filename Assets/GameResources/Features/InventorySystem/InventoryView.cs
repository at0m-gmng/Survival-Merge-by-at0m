using GameResources.Features.EditorGridDrawled;
using GameResources.Features.InventorySystem.Data;

namespace GameResources.Features.InventorySystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class InventoryView : MonoBehaviour
    {
        [Header("Config")]
        [field: SerializeField] public InventoryData Inventory { get; private set; } = default;
        
        [Header("Spawner")]
        [field: SerializeField] public GridLayoutGroup GridLayout { get; private set; } = default;
        [field: SerializeField] public RectTransform ItemParent { get; private set; } = default;

        private List<List<GameObject>> _cellObjects = new();
        private List<List<Wrapper<CellType>>> _cellWrappers = new();
        
        private void Start()
        {
            BuildGrid(Inventory.Rows, Inventory.Columns);
            CreateField();
            ItemParent.SetAsLastSibling();
            PlaceBaseItems();
            DebugGrid();
        }

        private void CreateField()
        {
            _cellObjects = new List<List<GameObject>>(Inventory.Rows);
            _cellWrappers = new List<List<Wrapper<CellType>>>(Inventory.Columns);

            for (int i = 0; i < Inventory.Rows; i++)
            {
                List<GameObject> rowObjects = new List<GameObject>(Inventory.Columns);
                List<Wrapper<CellType>> rowWrappers = new List<Wrapper<CellType>>(Inventory.Columns);

                for (int j = 0; j < Inventory.Columns; j++)
                {
                    GameObject go = Instantiate(Inventory.CellPrefab, GridLayout.transform);
                    go.name = $"{Inventory.CellPrefab.name}_{i}_{j}";

                    Wrapper<CellType> wrapper = new Wrapper<CellType>
                    {
                        Values = new[] { CellType.Empty }
                    };

                    rowObjects.Add(go);
                    rowWrappers.Add(wrapper);
                }

                _cellObjects.Add(rowObjects);
                _cellWrappers.Add(rowWrappers);
            }
        }

        private void BuildGrid(int rows, int cols)
        {
            bool isFixRows = rows <= cols;
            GridLayout.constraint = isFixRows ? GridLayoutGroup.Constraint.FixedRowCount : GridLayoutGroup.Constraint.FixedColumnCount;
            GridLayout.constraintCount = isFixRows ? rows : cols;
        }

        private void PlaceBaseItems()
        {
            for (int i = 0; i < Inventory.BaseItems.Length; i++)
            {
                var shape = Inventory.BaseItems[i].TryGetItemSize();
                if (shape == null || shape.Length == 0) continue;

                if (!TryFindPosition(shape, out int posRow, out int posCol))
                {
                    Debug.LogWarning($"No space for base item {i}");
                    continue;
                }

                OccupyCells(shape, posRow, posCol);

                var center = FindCenter(shape);
                int centerRow = posRow + center.X;
                int centerCol = posCol + center.Y;

                int shapeRows = shape.Length;
                int shapeCols = shape[0].Values.Length;

                var pivot = new Vector2(
                    (center.Y + 0.5f) / shapeCols,
                    1f - (center.X + 0.5f) / shapeRows
                );

                var worldCenter = GetCellCenterWorld(centerRow, centerCol);

                var itemPrefab = Inventory.BaseItems[i].UIPrefab;
                var created = Instantiate(itemPrefab);
                var rect = created.GetComponent<RectTransform>();

                rect.SetParent(ItemParent, false);
                rect.pivot = pivot;
                rect.sizeDelta = new Vector2(shapeCols * GridLayout.cellSize.x, shapeRows * GridLayout.cellSize.y);
                rect.position = worldCenter;
            }
        }
        
        private bool TryFindPosition(Wrapper<CellType>[] shape, out int posRow, out int posCol)
        {
            int invRows = _cellWrappers.Count;
            int invCols = _cellWrappers[0].Count;
            int shapeRows = shape.Length;
            int shapeCols = shape[0].Values.Length;

            for (int i = 0; i <= invRows - shapeRows; i++)
            {
                for (int j = 0; j <= invCols - shapeCols; j++)
                {
                    if (CanPlaceAt(shape, i, j))
                    {
                        posRow = i;
                        posCol = j;
                        return true;
                    }
                }
            }

            posRow = posCol = 0;
            return false;
        }

        private bool CanPlaceAt(Wrapper<CellType>[] shape, int posRow, int posCol)
        {
            int shapeRows = shape.Length;
            int shapeCols = shape[0].Values.Length;

            for (int si = 0; si < shapeRows; si++)
            {
                for (int sj = 0; sj < shapeCols; sj++)
                {
                    if (shape[si].Values[sj] != CellType.Empty)
                    {
                        if (_cellWrappers[posRow + si][posCol + sj].Values[0] != CellType.Empty)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void OccupyCells(Wrapper<CellType>[] shape, int posRow, int posCol)
        {
            int shapeRows = shape.Length;
            int shapeCols = shape[0].Values.Length;

            for (int si = 0; si < shapeRows; si++)
            {
                for (int sj = 0; sj < shapeCols; sj++)
                {
                    if (shape[si].Values[sj] != CellType.Empty)
                    {
                        _cellWrappers[posRow + si][posCol + sj].Values[0] = CellType.Busy;
                    }
                }
            }
        }

        private Utils.Vector2Int FindCenter(Wrapper<CellType>[] shape)
        {
            int rows = shape.Length;
            if (rows == 0) return Utils.Vector2Int.zero;

            int cols = shape[0].Values.Length;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < shape[i].Values.Length; j++)
                {
                    if (shape[i].Values[j] == CellType.Center)
                    {
                        return new Utils.Vector2Int(i, j);
                    }
                }
            }

            Debug.LogWarning("No center found, using (0,0)");
            return Utils.Vector2Int.zero;
        }
        private Vector3 GetCellCenterWorld(int row, int col)
        {
            RectOffset pad = GridLayout.padding;
            Vector2 size = GridLayout.cellSize;
            Vector2 spacing = GridLayout.spacing;

            float x = pad.left + col * (size.x + spacing.x) + size.x * 0.5f;
            float y = -(pad.top + row * (size.y + spacing.y) + size.y * 0.5f);

            Vector3 gridLocal = new Vector3(x, y, 0f);
            return GridLayout.transform.TransformPoint(gridLocal);
        }

        private void DebugGrid()
        {
            for (int i = 0; i < Inventory.Rows; i++)
            {
                for (int j = 0; j < Inventory.Columns; j++)
                {
                    Debug.Log($"{_cellObjects[i][j].name}: {_cellWrappers[i][j].Values[0]}");
                }
            }
        }
    }
}