namespace GameResources.Features.InventorySystem.Data
{
    using System;
    using UnityEngine;

    [Serializable]
    public class ItemDataView
    {
        public string Name;
        public int Level;
        public Sprite Image;
        public DamageType DamageType;
        public TargetType TargetType;
        public float AttackSpeed;
        public float Damage;
        public float Range;
    }

    public enum DamageType
    {
        Physical = 0,
        Magical = 1
    }
    public enum TargetType
    {
        Random = 0,
        Nearest = 1
    }
}