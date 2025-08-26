namespace GameResources.Features.InventorySystem
{
    using EditorGridDrawled;
    using Data;
    using UniRx;
    using Matrix;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;

    public class InventoryView : MonoBehaviour
    {
        private const int MAX_ROTATION_COUNT = 4;
        
        public IReadOnlyReactiveProperty<bool> Initialized => _initialized;
        private readonly ReactiveProperty<bool> _initialized = new ReactiveProperty<bool>(false);

        [field: SerializeField] public InventoryData Inventory { get; private set; }
        [field: SerializeField] public GridLayoutGroup GridLayout { get; private set; }
        [field: SerializeField] public RectTransform GridLayoutRect { get; private set; }
        [field: SerializeField] public RectTransform ItemParent { get; private set; }
        [field: SerializeField] public RectTransform OutsideParent { get; private set; }

        private readonly List<List<RectTransform>> _cellObjects = new();
        private Dictionary<string, List<Vector2Int>> _busyCells = new Dictionary<string, List<Vector2Int>>();
        private Matrix _cellMatrix;

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

                    for (int k = 0; k < MAX_ROTATION_COUNT; k++)
                    {
                        BaseItem rotated = itemView.ItemData.GetRotation(k);

                        if (IsAvailablePlaceByCenter(rotated.Grid, cellPos))
                        {
                            itemView.ItemData = rotated;
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

        public bool IsAvailablePlaceByCenter(Matrix shape, Vector3 worldPosition)
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

            if ((targetRow == -1 || targetCol == -1) || (_cellMatrix.Rows[targetRow].Columns[targetCol] != Matrix.EMPTY))
            {
                return false;
            }

            Vector2Int shapeCenter = shape.GetItemCenter();

            int startRow = targetRow - shapeCenter.x;
            int startCol = targetCol - shapeCenter.y;

            for (int i = 0; i < shape.Rows.Count; i++)
            {
                for (int j = 0; j < shape.Rows[i].Columns.Count; j++)
                {
                    if (shape.Rows[i].Columns[j] != Matrix.EMPTY)
                    {
                        int checkRow = startRow + i;
                        if (checkRow < 0 || checkRow >= _cellMatrix.Rows.Count)
                        {
                            return false;
                        }

                        int colsInRow = _cellMatrix.Rows[checkRow].Columns.Count;
                        int checkCol = startCol + j;
                        if ((checkCol < 0 || checkCol >= colsInRow) || (_cellMatrix.Rows[checkRow].Columns[checkCol] != Matrix.EMPTY))
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
                Vector2Int shapeCenter = placement.Shape.GetItemCenter();
                int startItemRow = placement.ItemCenter.x - shapeCenter.x;
                int startItemCol = placement.ItemCenter.y - shapeCenter.y;

                if (AreCellsInBounds(startItemRow, startItemCol, placement.Shape))
                {
                    ReleaseCells(placement.PlacementCells, startItemRow, startItemCol);
                    Inventory.TryRemovePlacement(placement.ID);
                    return true;
                }
            }
            return false;
        }
        
        public bool TryRestorePlacement(PlacementItem placement)
        {
            if (placement != null)
            {
                Vector2Int shapeCenter = placement.Shape.GetItemCenter();
                int startItemRow = placement.ItemCenter.x - shapeCenter.x;
                int startItemCol = placement.ItemCenter.y - shapeCenter.y;

                if (AreCellsInBounds(startItemRow, startItemCol, placement.Shape))
                {
                    for (int i = 0; i < placement.Shape.Rows.Count; i++)
                    {
                        for (int j = 0; j < placement.Shape.Rows[i].Columns.Count; j++)
                        {
                            if (placement.Shape.Rows[i].Columns[j] != Matrix.EMPTY)
                            {
                                int nextRow = startItemRow + i;
                                int nextCol = startItemCol + j;
                                if (_cellMatrix.Rows[nextRow].Columns[nextCol] != Matrix.EMPTY)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    OccupyCells(placement.Shape, placement.PlacementCells, startItemRow, startItemCol);
                    Inventory.AddOrUpdatePlacement(placement);
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
            _cellMatrix = new Matrix { Rows = new List<ColumnList>(Inventory.Rows) };
            
            for (int i = 0; i < Inventory.Rows; i++)
            {
                List<RectTransform> rowObjects = new List<RectTransform>(Inventory.Columns);
                ColumnList matrixRow = new ColumnList {Columns = new List<int>(Inventory.Columns) };

                for (int j = 0; j < Inventory.Columns; j++)
                {
                    RectTransform gridCell = Instantiate(Inventory.CellPrefab, GridLayout.transform);
                    gridCell.gameObject.name = $"{Inventory.CellPrefab.name}_{i}_{j}";

                    rowObjects.Add(gridCell);
                    matrixRow.Columns.Add((int)CellType.Empty);
                }
                _cellObjects.Add(rowObjects);
                _cellMatrix.Rows.Add(matrixRow);
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
            for (int i = 0; i < Inventory.PlacedItems.Count; i++)
            {
                PlacementItem placement = Inventory.PlacedItems[i];
                if (placement != null)
                {
                    Vector2Int shapeCenter = placement.Shape.GetItemCenter();
                    int startRow = placement.ItemCenter.x - shapeCenter.x;
                    int startCol = placement.ItemCenter.y - shapeCenter.y;

                    if (AreCellsInBounds(startRow, startCol, placement.Shape))
                    {
                        OccupyCells(placement.Shape, placement.PlacementCells, startRow, startCol);
                    }
                }
            }
        }
        
        private void OccupyCells(Matrix shape, List<Vector2Int> vector2Ints, int posRow, int posCol)
        {
            vector2Ints.Clear();
            for (int i = 0; i < shape.Rows.Count; i++)
            {
                for (int j = 0; j < shape.Rows[i].Columns.Count; j++)
                {
                    if (shape.Rows[i].Columns[j] != Matrix.EMPTY)
                    {
                        _cellMatrix.Rows[posRow + i].Columns[posCol + j] = Matrix.BUSY;
                        vector2Ints.Add(new Vector2Int(posRow + i, posCol + j));
                    }
                }
            }
        }
        
        private void ReleaseCells(List<Vector2Int> placement, int posRow, int posCol)
        {
            for (int k = 0; k < placement.Count; k++)
            {
                for (int i = 0; i < _cellMatrix.Rows.Count; i++)
                {
                    for (int j = 0; j < _cellMatrix.Rows[i].Columns.Count; j++)
                    {
                        if (placement[k].x == i && placement[k].y == j)
                        {
                            _cellMatrix.Rows[i].Columns[j] = Matrix.EMPTY;
                            break;
                        }
                    }
                }
            }
        }
        
        private bool TryPlaceItem(ItemView itemView, Vector3 position, Matrix shape = default)
        {
            if (shape.Rows == null || shape.Rows.Count == 0)
            {
                shape = Matrix.Clone(itemView.ItemData.Grid);
            }

            if (TryGetPlacementByCenter(shape, position, out int startRow, out int startCol, out int centerRow, out int centerCol))
            {
                List<Vector2Int> busyCells = new List<Vector2Int>();
                OccupyCells(shape, busyCells, startRow, startCol);

                itemView.Rect.SetParent(ItemParent);
                itemView.transform.localPosition = ItemParent.InverseTransformPoint(_cellObjects[centerRow][centerCol].transform.position);
                
                PlacementItem newPlacement = new PlacementItem
                {
                    ID = itemView.ID,
                    ItemCenter = new Vector2Int(centerRow, centerCol),
                    PlacementCells = busyCells,
                    Shape = Matrix.Clone(shape)
                };
                Inventory.AddOrUpdatePlacement(newPlacement);
                return true;
            }
            return false;
        }
        
        private bool TryGetPlacementByCenter(Matrix shape, Vector3 position, out int startItemRow, out int startItemCol, out int centerRow, out int centerCol)
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
                Vector2Int shapeCenter = shape.GetItemCenter();
                startItemRow = centerRow - shapeCenter.x;
                startItemCol = centerCol - shapeCenter.y;

                for (int i = 0; i < shape.Rows.Count; i++)
                {
                    for (int j = 0; j < shape.Rows[i].Columns.Count; j++)
                    {
                        if (shape.Rows[i].Columns[j] != Matrix.EMPTY)
                        {
                            int nextRow = startItemRow + i;
                            int nextCol = startItemCol + j;

                            if ((nextRow < 0 || nextRow >= _cellMatrix.Rows.Count) || 
                                (nextCol < 0 || nextCol >= _cellMatrix.Rows[nextRow].Columns.Count) || 
                                (_cellMatrix.Rows[nextRow].Columns[nextCol] != Matrix.EMPTY))
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
        
        private bool AreCellsInBounds(int startRow, int startCol, Matrix shape)
        {
            if (startRow >= 0 && startRow + shape.Rows.Count <= _cellMatrix.Rows.Count)
            {
                for (int i = 0; i < shape.Rows.Count; i++)
                {
                    int nextRow = startRow + i;
                    if ((nextRow < 0 || nextRow >= _cellMatrix.Rows.Count) || 
                        (startCol < 0 || startCol + shape.Rows[i].Columns.Count > _cellMatrix.Rows[nextRow].Columns.Count))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}
