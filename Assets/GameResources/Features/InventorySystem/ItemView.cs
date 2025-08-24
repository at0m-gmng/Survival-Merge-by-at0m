namespace GameResources.Features.InventorySystem
{
    using Data;
    using DG.Tweening;
    using UniRx;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using MergeData;

    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public virtual BaseItem ItemData { get; private set; }
        public string ID { get; private set; } = string.Empty;

        [field: SerializeField] public RectTransform Rect { get; private set; } = default;
        [SerializeField] private Image _image = default;
        
        private InventoryView _inventoryView = default;
        private MergeData _mergeData = default;
        private Vector3 _startPosition;
        private Transform _startParent;
        private CompositeDisposable _dragDisposables = new CompositeDisposable();
        private float _rotationAngle;
        private BaseItem _tempData = default;
        private Vector3 _defaultRotation = default;
        private int _rotationCount = 0;

        #region UNITY_REGION

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = transform.position;
            _startParent = transform.parent;
            _defaultRotation = Rect.localEulerAngles;
            _tempData = ItemData;

            if (_inventoryView.Inventory.TryGetPlacement(ID, out PlacementItem placement))
            {
                if (_inventoryView.TryReleasePlacement(placement))
                {
                    _inventoryView.Inventory.TryRemovePlacement(ID);
                }
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

            if (!_inventoryView.IsAvailablePlaceByCenter(_tempData.TryGetItemSize(), transform.position))
            {
                transform.SetParent(_startParent);
                transform.position = _startPosition;

                Rect.eulerAngles = _defaultRotation;

                if (_inventoryView.Inventory.TryGetPlacement(ID, out PlacementItem placement))
                {
                    _inventoryView.TryRestorePlacement(placement);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_tempData.Id))
                {
                    ItemData.SaveRotation(_tempData);
                    _tempData = default;
                }
                _inventoryView.TryPlaceItem(this);
            }
            _image.color = Color.white;
        }

        #endregion

        #region PUBLIC_REGION

        public void Initialize(BaseItem itemData, MergeData mergeData, InventoryView inventoryView)
        {
            ItemData = itemData;
            _inventoryView = inventoryView;
            _mergeData = mergeData;
            ID = GetInstanceID().ToString();
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
            _rotationCount = (((_rotationCount + 1) % 4) + 4) % 4;
            _rotationAngle -= 90f;
            _tempData =  ItemData.GetRotation(_rotationCount);

            Rect.DORotate(new Vector3(0, 0, _rotationAngle), 0.1f);
            Rect.DOMove(Input.mousePosition, 0.1f);

            CheckAvaillablePlaced();
        }

        #endregion
    }
}