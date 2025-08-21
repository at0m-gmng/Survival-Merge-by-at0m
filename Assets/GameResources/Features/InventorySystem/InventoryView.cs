using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GameResources.Features.InventorySystem
{
    using Data;
    using EditorGridDrawled;

    public class InventoryView : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<bool> Initialized => _initialized;
        private readonly ReactiveProperty<bool> _initialized = new ReactiveProperty<bool>(false);
        
        [field: SerializeField] public InventoryData Inventory { get; private set; }
        [field: SerializeField] public GridLayoutGroup GridLayout { get; private set; }
        [field: SerializeField] public RectTransform ItemParent { get; private set; }

        private readonly List<List<GameObject>> _cellObjects = new();
        private readonly List<List<Wrapper<CellType>>> _cellWrappers = new();

        private void Start()
        {
            BuildGrid(Inventory.Rows, Inventory.Columns);
            CreateField();
            ItemParent.SetAsLastSibling();
            _initialized.Value = true;
        }

        public bool TryPlaceItem(ItemView prefab)
        {
            Wrapper<CellType>[] shape = prefab.ItemData.TryGetItemSize();
            if (shape != null && shape.Length != 0)
            {
                if (TryFindPosition(shape, out int posRow, out int posCol))
                {
                    OccupyCells(shape, posRow, posCol);

                    Vector2Int center = FindCenter(shape);
                    int centerRow = posRow + center.x;
                    int centerCol = posCol + center.y;

                    int shapeRows = shape.Length;
                    int shapeCols = shape[0].Values.Length;

                    Vector2 pivot = new Vector2(
                        (center.y + 0.5f) / shapeCols,
                        1f - (center.x + 0.5f) / shapeRows
                    );

                    Vector3 worldCenter = GetCellCenterWorld(centerRow, centerCol);

                    RectTransform rect = prefab.GetComponent<RectTransform>();
                    rect.pivot = pivot;
                    rect.sizeDelta = new Vector2(shapeCols * GridLayout.cellSize.x, shapeRows * GridLayout.cellSize.y);
                    rect.position = worldCenter;

                    return true;
                }

                return false;
            }

            return false;
        }
        
        public Vector2 CalculateItemRectSize(ItemData data)
        {
            Wrapper<CellType>[] shape = data.TryGetItemSize();
            if (shape != null && shape.Length != 0)
            {
                int rows = shape.Length;
                int cols = shape[0].Values.Length;

                return new Vector2(cols * GridLayout.cellSize.x, rows * GridLayout.cellSize.y);
            }

            return Vector2.zero;
        }
        
        private void CreateField()
        {
            _cellObjects.Clear();
            _cellWrappers.Clear();

            for (int i = 0; i < Inventory.Rows; i++)
            {
                List<GameObject> rowObjects = new List<GameObject>(Inventory.Columns);
                List<Wrapper<CellType>> rowWrappers = new List<Wrapper<CellType>>(Inventory.Columns);

                for (int j = 0; j < Inventory.Columns; j++)
                {
                    GameObject go = Instantiate(Inventory.CellPrefab, GridLayout.transform);
                    go.name = $"{Inventory.CellPrefab.name}_{i}_{j}";
                    rowObjects.Add(go);
                    rowWrappers.Add(new Wrapper<CellType> { Values = new[] { CellType.Empty } });
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

        private bool TryFindPosition(Wrapper<CellType>[] shape, out int posRow, out int posCol)
        {
            posRow = posCol = 0;
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

            return false;
        }

        private bool CanPlaceAt(Wrapper<CellType>[] shape, int posRow, int posCol)
        {
            int sr = shape.Length;
            for (int si = 0; si < sr; si++)
            {
                int sc = shape[si].Values.Length;
                for (int sj = 0; sj < sc; sj++)
                {
                    if (shape[si].Values[sj] != CellType.Empty && _cellWrappers[posRow + si][posCol + sj].Values[0] != CellType.Empty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void OccupyCells(Wrapper<CellType>[] shape, int posRow, int posCol)
        {
            int sr = shape.Length;
            for (int si = 0; si < sr; si++)
            {
                int sc = shape[si].Values.Length;
                for (int sj = 0; sj < sc; sj++)
                {
                    if (shape[si].Values[sj] != CellType.Empty)
                    {
                        _cellWrappers[posRow + si][posCol + sj].Values[0] = CellType.Busy;
                    }
                }
            }
        }

        private Vector2Int FindCenter(Wrapper<CellType>[] shape)
        {
            int rows = shape.Length;
            int cols = shape[0].Values.Length;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (shape[i].Values[j] == CellType.Center)
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }

            return Vector2Int.zero;
        }

        private Vector3 GetCellCenterWorld(int row, int col)
        {
            RectOffset pad = GridLayout.padding;
            Vector2 size = GridLayout.cellSize;
            Vector2 spacing = GridLayout.spacing;
            float x = pad.left + col * (size.x + spacing.x) + size.x * 0.5f;
            float y = -(pad.top + row * (size.y + spacing.y) + size.y * 0.5f);
            Vector3 local = new Vector3(x, y, 0f);
            return GridLayout.transform.TransformPoint(local);
        }
        
        private void DebugOccupiedCells(Wrapper<CellType>[] shape, int startRow, int startCol)
        {
            for (int i = 0; i < shape.Length; i++)
            {
                for (int j = 0; j < shape[i].Values.Length; j++)
                {
                    if (shape[i].Values[j] != CellType.Empty)
                    {
                        var cell = _cellObjects[startRow + i][startCol + j];
                        Debug.Log($"[OCCUPY] Row: {startRow + i}, Col: {startCol + j}, Cell: {cell.name}, GO: {cell}");
                    }
                }
            }
        }
    }
}
