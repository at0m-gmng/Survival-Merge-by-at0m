namespace GameResources.Features.InventorySystem
{
    using Data;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public virtual ItemData ItemData { get; private set; } = null;

        [field: SerializeField] public RectTransform Rect { get; private set; } = default;
        [SerializeField] private Image _image = default;
        
        private InventoryView _inventoryView = default;
        private Vector3 _startPosition;
        private Transform _startParent;
        
        public void Initialize(ItemData itemData, InventoryView inventoryView)
        {
            ItemData = itemData;
            _inventoryView = inventoryView;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = transform.position;
            _startParent = transform.parent;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_inventoryView.IsAvailablePlaceByCenter(this))
            {
                _image.color = Color.green;
            }
            else
            {
                _image.color = Color.red;
            }
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_inventoryView.IsAvailablePlaceByCenter(this))
            {
                transform.SetParent(_startParent);
                transform.position = _startPosition;
                _image.color = Color.red;
            }
            else
            {
                _inventoryView.TryPlaceItem(this);
            }
            _image.color = Color.white;
        }
    }
}