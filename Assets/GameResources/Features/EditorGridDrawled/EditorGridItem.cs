namespace GameResources.Features.EditorGridDrawled
{
    using System;
    using System.Collections.Generic;
    using Matrix;
    using UnityEngine;

    [Serializable]
    public sealed class EditorGridItem
    {
        public Matrix MatrixGrid => _matrixGrid;

        [SerializeField] private int _rows = 4;
        [SerializeField] private int _columns = 4;
        [Range(1, 10)]
        [SerializeField] private int _cellSize = 1;
        [SerializeField] private Matrix _matrixGrid;

        public void ResetGrid()
        {
            var mg = new Matrix { Rows = new List<ColumnList>() };
            int targetRows = Mathf.Max(1, _rows);
            int targetColumns = Mathf.Max(1, _columns);

            for (int i = 0; i < targetRows; i++)
            {
                var row = new ColumnList { Columns = new List<int>() };
                for (int j = 0; j < targetColumns; j++)
                    row.Columns.Add(0);
                mg.Rows.Add(row);
            }

            _matrixGrid = mg;
        }

        public Matrix GetGrid() => _matrixGrid;
    }

    public enum CellType
    {
        Empty = 0,
        Busy = 1,
        Center = 2
    }
}