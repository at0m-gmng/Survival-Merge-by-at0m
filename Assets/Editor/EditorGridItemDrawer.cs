#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace GameResources.Features.EditorGridDrawled
{
    using System.Collections;
    using System.Reflection;

    [CustomPropertyDrawer(typeof(EditorGridItem))]
    public sealed class EditorGridItemDrawer : PropertyDrawer
    {
        private const string ROWS = "_rows";
        private const string COLUMNS = "_columns";
        private const string CELL_SIZE = "_cellSize";
        private const string GRID_FIELD = "_matrixGrid";
        private const string MATRIX_ROWS = "Rows";
        private const string MATRIX_COLUMNS = "Columns";

        private SerializedProperty _matrixProperty;
        private SerializedProperty _rowsProperty;
        private SerializedProperty _columnsProperty;
        private SerializedProperty _cellSizeProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            _rowsProperty = property.FindPropertyRelative(ROWS);
            _columnsProperty = property.FindPropertyRelative(COLUMNS);
            _cellSizeProperty = property.FindPropertyRelative(CELL_SIZE);
            _matrixProperty = property.FindPropertyRelative(GRID_FIELD);

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
            int rows = _rowsProperty != null ? _rowsProperty.intValue : 0;
            int columns = _columnsProperty != null ? _columnsProperty.intValue : 0;
            int cellSizeMultiplier = _cellSizeProperty != null ? Mathf.Max(1, _cellSizeProperty.intValue) : 1;

            if (rows <= 0 || columns <= 0)
            {
                return;
            }

            if (_matrixProperty == null)
            {
                EditorGridItem editorGridItem = fieldInfo.GetValue(GetParentValue(property)) as EditorGridItem;
                editorGridItem?.ResetGrid();
                return;
            }

            SerializedProperty rowsArray = _matrixProperty.FindPropertyRelative(MATRIX_ROWS);
            if (rowsArray == null || !rowsArray.isArray || rowsArray.arraySize != rows)
            {
                EditorGridItem editorGridItem = fieldInfo.GetValue(GetParentValue(property)) as EditorGridItem;
                editorGridItem?.ResetGrid();
                return;
            }

            float baseCellSize = Mathf.Min(position.width / (float)columns, position.height / (float)rows);
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
                    SerializedProperty rowProp = rowsArray.GetArrayElementAtIndex(i).FindPropertyRelative(MATRIX_COLUMNS);

                    if (rowProp == null || !rowProp.isArray || rowProp.arraySize != columns)
                    {
                        EditorGridItem editorGridItem = fieldInfo.GetValue(GetParentValue(property)) as EditorGridItem;
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

                        SerializedProperty element = rowProp.GetArrayElementAtIndex(j);
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

        public static object GetParentValue(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data[", "[");
            string[] elements = path.Split('.');
            foreach (string element in elements[..^1])
            {
                if (element.Contains("["))
                {
                    string elementName = element[..element.IndexOf("[")];
                    int index = Convert.ToInt32(element[(element.IndexOf("[") + 1)..^1]);
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source != null)
            {
                Type type = source.GetType();
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                return field?.GetValue(source);
            }
            return null;
        }

        private static object GetValue(object source, string name, int index)
        {
            IEnumerable enumerable = GetValue(source, name) as IEnumerable;
            if (enumerable != null)
            {
                IEnumerator enm = enumerable.GetEnumerator();
                for (int i = 0; i <= index; i++)
                {
                    enm.MoveNext();
                }
                return enm.Current;
            }
            return null;
        }
    }
}
#endif
