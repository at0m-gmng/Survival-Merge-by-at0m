namespace GameResources.Features.Matrix
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public struct Matrix
    {
        public const int EMPTY = 0;
        public const int BUSY = 1;
        public const int CENTER = 2;
        
        public List<ColumnList> Rows;
        
        public static Matrix Clone(Matrix source)
        {
            if (source.Rows != null)
            {
                Matrix clone = new Matrix { Rows = new List<ColumnList>(source.Rows.Count) };
                for (int i = 0; i < source.Rows.Count; i++)
                {
                    ColumnList newRow = new ColumnList { Columns = new List<int>(source.Rows[i].Columns.Count) };
                    for (int j = 0; j < source.Rows[i].Columns.Count; j++)
                    {
                        newRow.Columns.Add(source.Rows[i].Columns[j]);
                    }
                    clone.Rows.Add(newRow);
                }
                return clone;
            }
            return new Matrix { Rows = new List<ColumnList>() };
        }
        
        public Vector2Int GetItemCenter()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                for (int j = 0; j < Rows[i].Columns.Count; j++)
                {
                    if (Rows[i].Columns[j] == Matrix.CENTER)
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }
            return Vector2Int.zero;
        }

        public Matrix Rotate(int rotationSteps)
        {
            rotationSteps %= 4;
            if (rotationSteps != 0)
            {
                Matrix rotated = this;
                for (int i = 0; i < rotationSteps; i++)
                {
                    rotated = Clone(rotated.Rotate90Clockwise());
                }
                return rotated;
            }
            return this;
        }

        private Matrix Rotate90Clockwise()
        {
            if (Rows != null && Rows.Count != 0)
            {
                Matrix rotated = new Matrix { Rows = new List<ColumnList>() };

                for (int j = 0; j < Rows[0].Columns.Count; j++)
                {
                    ColumnList newRow = new ColumnList { Columns = new List<int>() };
                    for (int i = Rows.Count - 1; i >= 0; i--)
                    {
                        newRow.Columns.Add(Rows[i].Columns[j]);
                    }
                    rotated.Rows.Add(newRow);
                }
                return rotated;
            }
            return new Matrix { Rows = new List<ColumnList>() };
        }
    }
}