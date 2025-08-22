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
        [field: SerializeField] public RectTransform GridLayoutRect { get; private set; }
        [field: SerializeField] public RectTransform ItemParent { get; private set; }
        [field: SerializeField] public RectTransform OutsideParent { get; private set; }

        private readonly List<List<RectTransform>> _cellObjects = new();
        private readonly List<List<Wrapper<CellType>>> _cellWrappers = new();

        private void Start()
        {
            BuildGrid(Inventory.Rows, Inventory.Columns);
            CreateField();
            ItemParent.SetAsLastSibling();
            OutsideParent.SetAsLastSibling();
            _initialized.Value = true;
        }

        public bool TryAutoPlaceItem(ItemView itemView)
        {
            for (int i = 0; i < _cellObjects.Count; i++)
            {
                for (int j = 0; j < _cellObjects[i].Count; j++)
                {
                    if (IsAvailablePlaceByCenter(itemView.ItemData.TryGetItemSize(), _cellObjects[i][j].transform.position))
                    {
                        TryPlaceItem(itemView, _cellObjects[i][j].transform.position);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryPlaceItem(ItemView itemView) => TryPlaceItem(itemView, itemView.transform.position);

        private bool TryPlaceItem(ItemView itemView, Vector3 position)
        {
            Wrapper<CellType>[] shape = itemView.ItemData.TryGetItemSize();
            if (TryGetPlacementByCenter(shape, position, out int startRow, out int startCol, out int centerRow, out int centerCol))
            {
                OccupyCells(shape, startRow, startCol);

                itemView.Rect.SetParent(ItemParent);
                itemView.Rect.sizeDelta = new Vector2(shape[0].Values.Length * GridLayout.cellSize.x, shape.Length * GridLayout.cellSize.y);
                itemView.transform.localPosition = ItemParent.InverseTransformPoint(_cellObjects[centerRow][centerCol].transform.position);
                
                return true;
            }
            return false;
        }

        public bool IsAvailablePlaceByCenter(Wrapper<CellType>[] shape, Vector3 worldPosition)
        {
            Vector2 localItemPos = GridLayout.transform.InverseTransformPoint(worldPosition);

            int targetRow = -1, targetCol = -1;
            float minDistSq = 0.025f * (GridLayout.cellSize.x * GridLayout.cellSize.x + GridLayout.cellSize.y * GridLayout.cellSize.y);

            for (int i = 0; i < _cellObjects.Count; i++)
            {
                for (int j = 0; j < _cellObjects[i].Count; j++)
                {
                    Vector2 localCellPos = GridLayout.transform.InverseTransformPoint(_cellObjects[i][j].transform.position);
                    float dx = localItemPos.x - localCellPos.x;
                    float dy = localItemPos.y - localCellPos.y;
                    float distSq = dx * dx + dy * dy;
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        targetRow = i;
                        targetCol = j;
                    }
                }
            }

            if ((targetRow == -1 || targetCol == -1) || (_cellWrappers[targetRow][targetCol].Values[0] != CellType.Empty)) 
            {
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
                        if ((checkCol < 0 || checkCol >= colsInRow) || (_cellWrappers[checkRow][checkCol].Values[0] != CellType.Empty))
                        {
                            return false;
                        }
                    }
                }
            }


            return true;
        }

        public bool IsAvailablePlaceByCenter(ItemView itemView) 
            => IsAvailablePlaceByCenter(itemView.ItemData.TryGetItemSize(), itemView.transform.position);

        private void CreateField()
        {
            _cellObjects.Clear();
            _cellWrappers.Clear();

            for (int i = 0; i < Inventory.Rows; i++)
            {
                List<RectTransform> rowObjects = new List<RectTransform>(Inventory.Columns);
                List<Wrapper<CellType>> rowWrappers = new List<Wrapper<CellType>>(Inventory.Columns);

                for (int j = 0; j < Inventory.Columns; j++)
                {
                    RectTransform go = Instantiate(Inventory.CellPrefab, GridLayout.transform);
                    go.gameObject.name = $"{Inventory.CellPrefab.name}_{i}_{j}";
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
                for (int j = 0; j < shape[i].Values.Length; j++)
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
        

        private bool TryGetPlacementByCenter(Wrapper<CellType>[] shape, Vector3 position, out int startRow, out int startCol, out int centerRow, out int centerCol)
        {
            startRow = startCol = centerRow = centerCol = -1;

            Vector2 localItemPos = GridLayout.transform.InverseTransformPoint(position);
            float minDistSq = float.MaxValue;
            for (int i = 0; i < _cellObjects.Count; i++)
            {
                for (int j = 0; j < _cellObjects[i].Count; j++)
                {
                    Vector2 localCellPos = GridLayout.transform.InverseTransformPoint(_cellObjects[i][j].transform.position);
                    float dx = localItemPos.x - localCellPos.x;
                    float dy = localItemPos.y - localCellPos.y;
                    float d2 = dx * dx + dy * dy;
                    if (d2 < minDistSq)
                    {
                        minDistSq = d2;
                        centerRow = i;
                        centerCol = j;
                    }
                }
            }

            if (centerRow >= 0)
            {
                Vector2Int shapeCenter = GetItemCenter(shape);
                startRow = centerRow - shapeCenter.x;
                startCol = centerCol - shapeCenter.y;

                for (int i = 0; i < shape.Length; i++)
                {
                    for (int j = 0; j < shape[i].Values.Length; j++)
                    {
                        if (shape[i].Values[j] != CellType.Empty)
                        {
                            int rr = startRow + i;
                            int cc = startCol + j;

                            if ((rr < 0 || rr >= _cellWrappers.Count) || (cc < 0 || cc >= _cellWrappers[rr].Count) || (_cellWrappers[rr][cc].Values[0] != CellType.Empty))
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