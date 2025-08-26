namespace GameResources.Features.EditorGridDrawled.Examples
{
    using Matrix;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ExampleEditorGridItem", menuName = "Example/Configs/ExampleEditorGridItem")]
    public sealed class ExampleEditorGridItem : ScriptableObject
    {
        [SerializeField] private Matrix _grid;

        [SerializeField] private EditorGridItem _editorGrid = new EditorGridItem();
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            _grid = _editorGrid.GetGrid();
        }
#endif
    }
}