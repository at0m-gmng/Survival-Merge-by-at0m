#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace GameResources.Features.EditorGridDrawled
{
    [CustomPropertyDrawer(typeof(EditorGridItem))]
    public sealed class EditorGridItemDrawer : PropertyDrawer
    {
        private const string ROWS = "_rows";
        private const string COLUMNS = "_columns";
        private const string CELL_SIZE = "_cellSize";
        private const string GRID_FIELD = "_grid";
        private const string WRAPPER_VALUES = "Values";

        private SerializedProperty _gridProperty;
        private SerializedProperty _rowsProperty;
        private SerializedProperty _columnsProperty;
        private SerializedProperty _cellSizeProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            _rowsProperty = property.FindPropertyRelative(ROWS);
            _columnsProperty = property.FindPropertyRelative(COLUMNS);
            _cellSizeProperty = property.FindPropertyRelative(CELL_SIZE);
            _gridProperty = property.FindPropertyRelative(GRID_FIELD);

            Rect rowsRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect columnsRect = new Rect(position.x, rowsRect.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
            Rect cellSizeRect = new Rect(position.x, columnsRect.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
        
            EditorGUI.PropertyField(rowsRect, _rowsProperty);
            EditorGUI.PropertyField(columnsRect, _columnsProperty);
            EditorGUI.PropertyField(cellSizeRect, _cellSizeProperty);

            Rect gridRect = new Rect(
                position.x, 
                cellSizeRect.y + EditorGUIUtility.singleLineHeight + 10, 
                position.width, 
                position.height - (EditorGUIUtility.singleLineHeight * 3 + 14)
            );

            DrawGrid(gridRect, property);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty rowsProperty = property.FindPropertyRelative(ROWS);
            SerializedProperty cellSizeProperty = property.FindPropertyRelative(CELL_SIZE);
        
            int rows = rowsProperty != null ? Mathf.Max(1, rowsProperty.intValue) : 4;
            int cellSizeMultiplier = cellSizeProperty != null ? Mathf.Max(1, cellSizeProperty.intValue) : 1;
        
            float gridHeight = rows * 30 * cellSizeMultiplier;
        
            return EditorGUIUtility.singleLineHeight * 3 + 14 + gridHeight;
        }

        private void DrawGrid(Rect position, SerializedProperty property)
        {
            int rows = _rowsProperty.intValue;
            int columns = _columnsProperty.intValue;
            int cellSizeMultiplier = Mathf.Max(1, _cellSizeProperty.intValue);

            if (rows <= 0 || columns <= 0)
            {
                return;
            }

            if (_gridProperty == null || !_gridProperty.isArray || _gridProperty.arraySize != rows)
            {
                EditorGridItem editorGridItem = fieldInfo.GetValue(property.serializedObject.targetObject) as EditorGridItem;
                editorGridItem?.ResetGrid();
                return;
            }

            float baseCellSize = Mathf.Min(position.width / columns, position.height / rows);
            float cellSize = baseCellSize * cellSizeMultiplier;
            cellSize = Mathf.Min(cellSize, baseCellSize);
        
            float gridWidth = cellSize * columns;
        
            Vector2 gridStart = new Vector2(
                position.x + (position.width - gridWidth) / 2,
                position.y
            );

            try
            {
                for (int i = 0; i < rows; i++)
                {
                    SerializedProperty row = _gridProperty.GetArrayElementAtIndex(i).FindPropertyRelative(WRAPPER_VALUES);
                
                    if (row.arraySize != columns)
                    {
                        EditorGridItem editorGridItem = fieldInfo.GetValue(property.serializedObject.targetObject) as EditorGridItem;
                        editorGridItem?.ResetGrid();
                        return;
                    }

                    for (int j = 0; j < columns; j++)
                    {
                        Rect cellRect = new Rect(
                            gridStart.x + j * cellSize,
                            gridStart.y + i * cellSize,
                            cellSize,
                            cellSize
                        );

                        if (cellRect.y > position.y + position.height || cellRect.y + cellSize < position.y)
                        {
                            continue;
                        }

                        SerializedProperty element = row.GetArrayElementAtIndex(j);
                        CellType currentCell = (CellType)element.intValue;

                        Color originalColor = GUI.color;
                        switch (element.intValue)
                        {
                            case 0:
                                GUI.color = Color.green;
                                break;
                            case 1:
                                GUI.color = Color.red;
                                break;
                            case 2:
                                GUI.color = Color.yellow;
                                break;
                        }
                    
                        if (GUI.Button(cellRect, currentCell.ToString()))
                        {
                            element.intValue = GetNextElement(element.intValue, 1, 0, 2);
                        }
                    
                        GUI.color = originalColor;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"EditorGridItemDrawer: {exception}");
            }
        }
        private int GetNextElement(int current, int add, int min, int max)
        {
            int range = max - min + 1;
            return min + ((current - min + add) % range + range) % range;
        }
    }
}
#endif