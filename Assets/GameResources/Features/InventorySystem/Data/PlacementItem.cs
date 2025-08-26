namespace GameResources.Features.InventorySystem.Data
{
    using System.Collections.Generic;
    using Matrix;
    using UnityEngine;

    [System.Serializable]
    public class PlacementItem
    {
        public string ID;
        public Vector2Int ItemCenter;
        public List<Vector2Int> PlacementCells = new List<Vector2Int>();
        public Matrix Shape;
    }
}