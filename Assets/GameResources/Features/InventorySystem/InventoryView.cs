namespace GameResources.Features.InventorySystem
{
    using EditorGridDrawled;
    using Data;
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
        private List<RectTransform> _rowObjects = new List<RectTransform>();
        private List<Wrapper<CellType>> _rowWrappers = new List<Wrapper<CellType>>();

        #region UNITY_REGION

        private void Start()
        {
            BuildGrid(Inventory.Rows, Inventory.Columns);
            CreateField();
            RestoreOccupancyFromData();
            ItemParent.SetAsLastSibling();
            OutsideParent.SetAsLastSibling();
            _initialized.Value = true;
        }

        #endregion

        #region PUBLIC_REGION

        public bool TryAutoPlaceItem(ItemView itemView)
        {
            for (int i = 0; i < _cellObjects.Count; i++)
            {
                for (int j = 0; j < _cellObjects[i].Count; j++)
                {
                    Vector3 cellPos = _cellObjects[i][j].transform.position;

                    for (int k = 0; k < 4; k++)
                    {
                        BaseItem rotated = itemView.ItemData.GetRotation(k);

                        if (IsAvailablePlaceByCenter(rotated.Grid, cellPos))
                        {
                            itemView.ItemData.SaveRotation(rotated);
                            if (TryPlaceItem(itemView, cellPos, rotated.Grid))
                            {
                                itemView.ApplyRotation(k);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool TryPlaceItem(ItemView itemView) => TryPlaceItem(itemView, itemView.transform.position);

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
        
        public bool TryReleasePlacement(PlacementItem placement)
        {
            if (placement != null)
            {
                Vector2Int shapeCenter = GetItemCenter(placement.Shape);
                int startItemRow = placement.ItemCenter.x - shapeCenter.x;
                int startItemCol = placement.ItemCenter.y - shapeCenter.y;

                if (AreCellsInBounds(startItemRow, startItemCol, placement.Shape))
                {
                    ReleaseCells(placement.Shape, startItemRow, startItemCol);
                    return true;
                }
            }
            return false;
        }
        
        public bool TryRestorePlacement(PlacementItem placement)
        {
            if (placement != null)
            {
                Vector2Int shapeCenter = GetItemCenter(placement.Shape);
                int startItemRow = placement.ItemCenter.x - shapeCenter.x;
                int startItemCol = placement.ItemCenter.y - shapeCenter.y;

                if (AreCellsInBounds(startItemRow, startItemCol, placement.Shape))
                {
                    for (int i = 0; i < placement.Shape.Length; i++)
                    {
                        for (int j = 0; j < placement.Shape[i].Values.Length; j++)
                        {
                            if (placement.Shape[i].Values[j] != CellType.Empty)
                            {
                                int nextRow = startItemRow + i;
                                int nextCol = startItemCol + j;
                                if (_cellWrappers[nextRow][nextCol].Values[0] != CellType.Empty)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    OccupyCells(placement.Shape, startItemRow, startItemCol);
                    return true;
                }
                return false;
            }
            return false;
        }

        #endregion

        #region PRIVATE_REGION

        private void CreateField()
        {
            _cellObjects.Clear();
            _cellWrappers.Clear();
            _rowObjects.Clear();
            _rowWrappers.Clear();

            for (int i = 0; i < Inventory.Rows; i++)
            {
                _rowObjects = new List<RectTransform>(Inventory.Columns);
                _rowWrappers = new List<Wrapper<CellType>>(Inventory.Columns);

                for (int j = 0; j < Inventory.Columns; j++)
                {
                    RectTransform gridCell = Instantiate(Inventory.CellPrefab, GridLayout.transform);
                    gridCell.gameObject.name = $"{Inventory.CellPrefab.name}_{i}_{j}";
                    Wrapper<CellType> cellType = new Wrapper<CellType>
                    {
                        Values = new[] { CellType.Empty }
                    };
                    _rowObjects.Add(gridCell);
                    _rowWrappers.Add(cellType);
                }
                _cellObjects.Add(_rowObjects);
                _cellWrappers.Add(_rowWrappers);
            }
        }

        private void BuildGrid(int rows, int cols)
        {
            bool isFixRows = rows <= cols;
            GridLayout.constraint = isFixRows ? GridLayoutGroup.Constraint.FixedRowCount : GridLayoutGroup.Constraint.FixedColumnCount;
            GridLayout.constraintCount = isFixRows ? rows : cols;
        }
        
        private void RestoreOccupancyFromData()
        {
            for (var i = 0; i < Inventory.PlacedItems.Count; i++)
            {
                var placement = Inventory.PlacedItems[i];
                if (placement != null && placement.Shape != null)
                {
                    Vector2Int shapeCenter = GetItemCenter(placement.Shape);
                    int startRow = placement.ItemCenter.x - shapeCenter.x;
                    int startCol = placement.ItemCenter.y - shapeCenter.y;

                    if (AreCellsInBounds(startRow, startCol, placement.Shape))
                    {
                        OccupyCells(placement.Shape, startRow, startCol);
                    }
                }
            }
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
        
        private void ReleaseCells(Wrapper<CellType>[] shape, int posRow, int posCol)
        {
            for (int i = 0; i < shape.Length; i++)
            {
                for (int j = 0; j < shape[i].Values.Length; j++)
                {
                    if (shape[i].Values[j] != CellType.Empty)
                    {
                        _cellWrappers[posRow + i][posCol + j].Values[0] = CellType.Empty;
                    }
                }
            }
        }
        
        private bool TryPlaceItem(ItemView itemView, Vector3 position, Wrapper<CellType>[] shape = null)
        {
            shape ??= itemView.ItemData.TryGetItemSize();
            bool hadPosition = Inventory.TryGetPlacement(itemView.ID, out PlacementItem oldPlacement);

            if (hadPosition)
            {
                if (TryReleasePlacement(oldPlacement))
                {
                    Inventory.TryRemovePlacement(itemView.ID);
                }
            }

            if (TryGetPlacementByCenter(shape, position, out int startRow, out int startCol, out int centerRow, out int centerCol))
            {
                OccupyCells(shape, startRow, startCol);

                itemView.Rect.SetParent(ItemParent);
                itemView.transform.localPosition = ItemParent.InverseTransformPoint(_cellObjects[centerRow][centerCol].transform.position);

                PlacementItem newPlacement = new PlacementItem
                {
                    ID = itemView.ID,
                    ItemCenter = new Vector2Int(centerRow, centerCol),
                    Shape = shape
                };
                Inventory.AddOrUpdatePlacement(newPlacement);
                return true;
            }
            else
            {
                if (hadPosition)
                {
                    if (oldPlacement != null)
                    {
                        if (TryRestorePlacement(oldPlacement))
                        {
                            Inventory.AddOrUpdatePlacement(oldPlacement);
                        }
                    }
                }
                return false;
            }
        }
        
        private bool TryGetPlacementByCenter(Wrapper<CellType>[] shape, Vector3 position, out int startItemRow, out int startItemCol, out int centerRow, out int centerCol)
        {
            startItemRow = startItemCol = centerRow = centerCol = -1;
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
                startItemRow = centerRow - shapeCenter.x;
                startItemCol = centerCol - shapeCenter.y;

                for (int i = 0; i < shape.Length; i++)
                {
                    for (int j = 0; j < shape[i].Values.Length; j++)
                    {
                        if (shape[i].Values[j] != CellType.Empty)
                        {
                            int nextRow = startItemRow + i;
                            int nextCol = startItemCol + j;

                            if ((nextRow < 0 || nextRow >= _cellWrappers.Count) || (nextCol < 0 || nextCol >= _cellWrappers[nextRow].Count) || (_cellWrappers[nextRow][nextCol].Values[0] != CellType.Empty))
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
        
        private bool AreCellsInBounds(int startRow, int startCol, Wrapper<CellType>[] shape)
        {
            if (startRow >= 0 && startRow + shape.Length <= _cellWrappers.Count)
            {
                for (int i = 0; i < shape.Length; i++)
                {
                    int nextRow = startRow + i;
                    if ((nextRow < 0 || nextRow >= _cellWrappers.Count) || (startCol < 0 || startCol + shape[i].Values.Length > _cellWrappers[nextRow].Count))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private Vector2Int GetItemCenter(Wrapper<CellType>[] shape)
        {
            for (int i = 0; i < shape.Length; i++)
            {
                for (int j = 0; j < shape[i].Values.Length; j++)
                {
                    if (shape[i].Values[j] == CellType.Center)
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }
            return Vector2Int.zero;
        }
        
        #endregion
    }
}
