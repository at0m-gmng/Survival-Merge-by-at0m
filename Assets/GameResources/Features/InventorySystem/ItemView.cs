namespace GameResources.Features.InventorySystem
{
    using System.Collections.Generic;
    using Data;
    using DG.Tweening;
    using UniRx;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using MergeData;

    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public virtual BaseItem ItemData { get; set; }
        public string ID { get; private set; } = string.Empty;

        [field: SerializeField] public RectTransform Rect { get; private set; } = default;
        [SerializeField] private Image _image = default;
        [SerializeField] private Image _rayImage = default;
        
        private InventoryView _inventoryView = default;
        private MergeData _mergeData = default;
        private Vector3 _startPosition;
        private Transform _startParent;
        private CompositeDisposable _dragDisposables = new CompositeDisposable();
        private float _rotationAngle;
        private BaseItem _tempData = default;
        private Vector3 _defaultRotation = default;
        private int _rotationCount = 0;
        private int _startRotationCount = 0;
        private List<RaycastResult> _rayResult = new List<RaycastResult>();
        private PlacementItem _placementItem = default;

        #region UNITY_REGION

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = transform.position;
            _startParent = transform.parent;
            _defaultRotation = Rect.localEulerAngles;
            _tempData = ItemData;
            _startRotationCount = _rotationCount;
            transform.SetAsLastSibling();

            if (_inventoryView.Inventory.TryGetPlacement(ID, out _placementItem))
            {
                _inventoryView.TryReleasePlacement(_placementItem);
            }

            if (ItemData.IsRotatable)
            {
                Observable.EveryUpdate()
                    .Where(_ => Input.GetKeyDown(KeyCode.R))
                    .Subscribe(_ => RotateClockwise())
                    .AddTo(_dragDisposables);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            CheckAvaillablePlaced();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragDisposables.Clear();

            if (ItemData.IsMergable)
            {
                ItemView overlappedItem = GetOverlappedItem();
                if (overlappedItem != null && CanMerge(overlappedItem, out ItemData resultItem))
                {
                    if (_inventoryView.Inventory.TryGetPlacement(ID, out PlacementItem placement))
                    {
                        _inventoryView.TryReleasePlacement(placement);
                    }
                    if (_inventoryView.Inventory.TryGetPlacement(overlappedItem.ID, out PlacementItem inventoryPlacement))
                    {
                        _inventoryView.TryReleasePlacement(inventoryPlacement);
                    }
                    
                    Merge(overlappedItem, resultItem);
                }
            }
            
            if (!_inventoryView.IsAvailablePlaceByCenter(_tempData.TryGetItemSize(), transform.position))
            {
                transform.SetParent(_startParent);
                transform.position = _startPosition;
                ApplyRotation(_startRotationCount);
                _inventoryView.TryRestorePlacement(_placementItem);
            }
            else
            {
                if (!string.IsNullOrEmpty(_tempData.Id))
                {
                    ItemData = _tempData;
                }
                _inventoryView.TryPlaceItem(this);
            }
            _image.color = Color.white;
            _tempData = default;
        }

        #endregion

        #region PUBLIC_REGION

        public void Initialize(BaseItem itemData, MergeData mergeData, InventoryView inventoryView)
        {
            ItemData = itemData;
            _inventoryView = inventoryView;
            _mergeData = mergeData;
            ID = GetInstanceID().ToString();
            _rayImage.alphaHitTestMinimumThreshold = 0.1f;
        }

        public void ApplyRotation(int rotationCount)
        {
            _rotationCount = rotationCount;
            _rotationAngle = -90f * _rotationCount;
            Rect.eulerAngles = new Vector3(0, 0, _rotationAngle); 
        }

        #endregion

        #region PRIVATE_REGION

        private void CheckAvaillablePlaced()
        {
            if (_inventoryView.IsAvailablePlaceByCenter(_tempData.TryGetItemSize(), transform.position))
            {
                _image.color = Color.green;
            }
            else
            {
                _image.color = Color.red;
            }
        }
        
        private void RotateClockwise()
        {
            _rotationCount = (_rotationCount + 1) & 3;
            _rotationAngle -= 90f;
            _tempData = _tempData.GetRotation(1);

            Rect.DORotate(new Vector3(0, 0, _rotationAngle), 0.1f);
            Rect.DOMove(Input.mousePosition, 0.1f);

            CheckAvaillablePlaced();
        }
        
        private ItemView GetOverlappedItem()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = transform.position
            };
            _rayResult.Clear();
            EventSystem.current.RaycastAll(pointerData, _rayResult);

            foreach (RaycastResult result in _rayResult)
            {
                ItemView itemView = result.gameObject.GetComponent<ItemView>();
                if (itemView != null && itemView != this && itemView.ItemData.IsMergable)
                {
                    return itemView;
                }
            }
            return null;
        }

        private bool CanMerge(ItemView otherItem, out ItemData resultItem)
        {
            resultItem = null;
            if (ItemData.Type == otherItem.ItemData.Type && ItemData.Level == otherItem.ItemData.Level)
            {
                foreach (MergeRule rule in _mergeData.Rules)
                {
                    if (rule.Type == ItemData.Type && rule.Level == ItemData.Level)
                    {
                        resultItem = rule.ResultItem;
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        
        private void Merge(ItemView otherItem, ItemData resultItem)
        {
            Destroy(otherItem.gameObject);
            Destroy(gameObject);

            ItemView newItemView = Instantiate(resultItem.Item.UIPrefab, transform.parent);
            newItemView.Initialize(resultItem.Item, _mergeData, _inventoryView);
            _inventoryView.TryAutoPlaceItem(newItemView);
        }
        
        #endregion
    }
}