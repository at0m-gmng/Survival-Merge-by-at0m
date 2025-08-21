namespace GameResources.Features.InventorySystem
{
    using Data;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public virtual ItemData ItemData { get; private set; } = null;

        public void Initialize(ItemData itemData)
        {
            ItemData = itemData;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }
    }
}