namespace GameResources.Features.InventorySystem
{
    using Data;
    using EditorGridDrawled;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;

    public class InventoryView : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<bool> Initialized => _initialized;
        private readonly ReactiveProperty<bool> _initialized = new ReactiveProperty<bool>(false);
        
        [field: SerializeField] public InventoryData Inventory { get; private set; }
        [field: SerializeField] public GridLayoutGroup GridLayout { get; private set; }
        [field: SerializeField] public RectTransform ItemParent { get; private set; }
        [field: SerializeField] public RectTransform OutsideParent { get; private set; }

        private readonly List<List<GameObject>> _cellObjects = new();
        private readonly List<List<Wrapper<CellType>>> _cellWrappers = new();

        private void Start()
        {
            BuildGrid(Inventory.Rows, Inventory.Columns);
            CreateField();
            ItemParent.SetAsLastSibling();
            OutsideParent.SetAsLastSibling();
            _initialized.Value = true;
        }

        public bool TryPlaceItem(ItemView itemView)
        {
            if (TryGetPlacementByCenter(itemView, out int startRow, out int startCol, out int centerRow, out int centerCol, out var shape))
            {
                OccupyCells(shape, startRow, startCol);

                Vector2Int center = GetItemCenter(shape);
                int shapeRows = shape.Length;
                int shapeCols = shape[0].Values.Length;

                Vector2 pivot = new Vector2((center.y + 0.5f) / shapeCols, 1f - (center.x + 0.5f) / shapeRows);

                itemView.Rect.SetParent(ItemParent, false);
                itemView.Rect.anchorMin = itemView.Rect.anchorMax = new Vector2(0.5f, 0.5f);
                itemView.Rect.pivot = pivot;
                itemView.Rect.sizeDelta = new Vector2(shapeCols * GridLayout.cellSize.x, shapeRows * GridLayout.cellSize.y);

                Vector3 localInParent = ItemParent.InverseTransformPoint(_cellObjects[centerRow][centerCol].transform.position);
                itemView.Rect.anchoredPosition = new Vector2(localInParent.x, localInParent.y);

                return true;
            }
            return false;
        }

        public bool IsAvailablePlaceByCenter(ItemView itemView)
        {
            Wrapper<CellType>[] shape = itemView.ItemData.TryGetItemSize();
            if (shape == null || shape.Length == 0)
            {
                Debug.Log("[IsAvailablePlaceByCenter] Предмет не имеет размера.");
                return false;
            }

            if (_cellObjects == null || _cellObjects.Count == 0 || _cellWrappers == null || _cellWrappers.Count == 0)
            {
                Debug.Log("[IsAvailablePlaceByCenter] Сетка не инициализирована.");
                return false;
            }

            Vector2 localItemPos = GridLayout.transform.InverseTransformPoint(itemView.transform.position);

            int targetRow = -1, targetCol = -1;
            float minDistSq = float.MaxValue;

            for (int r = 0; r < _cellObjects.Count; r++)
            {
                for (int c = 0; c < _cellObjects[r].Count; c++)
                {
                    Vector2 localCellPos = GridLayout.transform.InverseTransformPoint(_cellObjects[r][c].transform.position);
                    float dx = localItemPos.x - localCellPos.x;
                    float dy = localItemPos.y - localCellPos.y;
                    float distSq = dx * dx + dy * dy;
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        targetRow = r;
                        targetCol = c;
                    }
                }
            }

            if (targetRow == -1 || targetCol == -1)
            {
                Debug.Log("[IsAvailablePlaceByCenter] Не найдена целевая ячейка.");
                return false;
            }

            if (_cellWrappers[targetRow][targetCol].Values[0] != CellType.Empty)
            {
                Debug.Log($"[IsAvailablePlaceByCenter] Целевая ячейка занята: {targetRow},{targetCol}, ячейка иммет статус {_cellWrappers[targetRow][targetCol].Values[0]}");
                return false;
            }

            Vector2Int shapeCenter = GetItemCenter(shape);
            int startRow = targetRow - shapeCenter.x;
            int startCol = targetCol - shapeCenter.y;

            for (int i = 0; i < shape.Length; i++)
            {
                for (int j = 0; j < shape[i].Values.Length; j++)
                {
                    if (shape[i].Values[j] != CellType.Empty)
                    {
                        int checkRow = startRow + i;
                        if (checkRow < 0 || checkRow >= _cellWrappers.Count)
                        {
                            Debug.Log($"[IsAvailablePlaceByCenter] Выход за границы по строке: {checkRow}. target={targetRow},{targetCol} start={startRow},{startCol} shapeIndex={i},{j}");
                            return false;
                        }

                        int colsInRow = _cellWrappers[checkRow].Count;
                        int checkCol = startCol + j;
                        if (checkCol < 0 || checkCol >= colsInRow)
                        {
                            Debug.Log($"[IsAvailablePlaceByCenter] Выход за границы по столбцу: {checkCol}. target={targetRow},{targetCol} start={startRow},{startCol} shapeIndex={i},{j}");
                            return false;
                        }

                        if (_cellWrappers[checkRow][checkCol].Values[0] != CellType.Empty)
                        {
                            Debug.Log($"[IsAvailablePlaceByCenter] Ячейка занята: {checkRow},{checkCol}. target={targetRow},{targetCol} shapeIndex={i},{j}");
                            return false;
                        }
                    }
                }
            }
            return true;
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
                    Wrapper<CellType> cellType = new Wrapper<CellType> { Values = new[] { CellType.Empty } }; 
                    rowObjects.Add(go);
                    rowWrappers.Add(cellType);
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

        private void OccupyCells(Wrapper<CellType>[] shape, int posRow, int posCol)
        {
            for (int i = 0; i < shape.Length; i++)
            {
                int sc = shape[i].Values.Length;
                for (int j = 0; j < sc; j++)
                {
                    if (shape[i].Values[j] != CellType.Empty)
                    {
                        _cellWrappers[posRow + i][posCol + j].Values[0] = CellType.Busy;
                    }
                }
            }
        }

        private Vector2Int GetItemCenter(Wrapper<CellType>[] shape)
        {
            for (int i = 0; i < shape.Length; i++)
            {
                for (int j = 0; j < shape[0].Values.Length; j++)
                {
                    if (shape[i].Values[j] == CellType.Center)
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }
            return Vector2Int.zero;
        }
        

        private bool TryGetPlacementByCenter(ItemView itemView, out int startRow, out int startCol, out int centerRow, out int centerCol, out Wrapper<CellType>[] shape)
        {
            shape = itemView.ItemData.TryGetItemSize();
            startRow = startCol = centerRow = centerCol = -1;

            Vector2 localItemPos = GridLayout.transform.InverseTransformPoint(itemView.transform.position);

            float minDistSq = float.MaxValue;
            for (int r = 0; r < _cellObjects.Count; r++)
            {
                for (int c = 0; c < _cellObjects[r].Count; c++)
                {
                    Vector2 localCellPos = GridLayout.transform.InverseTransformPoint(_cellObjects[r][c].transform.position);
                    float dx = localItemPos.x - localCellPos.x;
                    float dy = localItemPos.y - localCellPos.y;
                    float d2 = dx * dx + dy * dy;
                    if (d2 < minDistSq)
                    {
                        minDistSq = d2;
                        centerRow = r;
                        centerCol = c;
                    }
                }
            }

            if (centerRow >= 0)
            {
                Vector2Int shapeCenter = GetItemCenter(shape);
                startRow = centerRow - shapeCenter.x;
                startCol = centerCol - shapeCenter.y;

                int rows = _cellWrappers.Count;
                for (int i = 0; i < shape.Length; i++)
                {
                    for (int j = 0; j < shape[i].Values.Length; j++)
                    {
                        if (shape[i].Values[j] != CellType.Empty)
                        {
                            int rr = startRow + i;
                            int cc = startCol + j;

                            if ((rr < 0 || rr >= rows) || (cc < 0 || cc >= _cellWrappers[rr].Count) || (_cellWrappers[rr][cc].Values[0] != CellType.Empty))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}