namespace GameResources.Features.EditorGridDrawled
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class EditorGridItem
    {
        [SerializeField] private int _rows = 4;
        [SerializeField] private int _columns = 4;
        [Range(1, 10)] 
        [SerializeField] private int _cellSize = 1;
        [SerializeField] private Wrapper<CellType>[] _grid;
    
        public void ResetGrid()
        {
            _grid = new Wrapper<CellType>[_rows];
            for (int i = 0; i < _rows; i++)
            {
                _grid[i] = new Wrapper<CellType>();
                _grid[i].Values = new CellType[_columns];
            
                for (int j = 0; j < _columns; j++)
                {
                    _grid[i].Values[j] = CellType.Empty;
                }
            }
        }

        public CellType[,] GetGridMatrix()
        {
            CellType[,] matrix = new CellType[_rows, _columns];
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    matrix[i, j] = _grid[i].Values[j];
                }
            }
            return matrix;
        }

        public Wrapper<CellType>[] GetGrid() => _grid;
    }

    [Serializable]
    public class Wrapper<T>
    {
        public T[] Values;
    }

    public enum CellType
    {
        Empty = 0, 
        Busy = 1,
        Center = 2
    }
}