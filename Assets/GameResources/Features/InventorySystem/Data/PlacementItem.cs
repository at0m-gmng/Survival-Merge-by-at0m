namespace GameResources.Features.InventorySystem.Data
{
    using EditorGridDrawled;
    using UnityEngine;

    [System.Serializable]
    public class PlacementItem
    {
        public string ID;
        public Vector2Int ItemCenter;
        public Wrapper<CellType>[] Shape;
    }
}