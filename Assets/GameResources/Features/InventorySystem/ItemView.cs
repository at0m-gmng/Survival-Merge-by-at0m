namespace GameResources.Features.InventorySystem
{
    using Data;
    using DG.Tweening;
    using UniRx;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using System.Linq;
    using EditorGridDrawled;

    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public virtual BaseItem ItemData { get; private set; }
        public string ID { get; private set; } = string.Empty;

        [field: SerializeField] public RectTransform Rect { get; private set; } = default;
        [SerializeField] private Image _image = default;
        
        private InventoryView _inventoryView = default;
        private Vector3 _startPosition;
        private Transform _startParent;
        private CompositeDisposable _dragDisposables = new CompositeDisposable();
        private Wrapper<CellType>[] _originalGrid;
        private float _rotationAngle;

        #region UNITY_REGION

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = transform.position;
            _startParent = transform.parent;
            _originalGrid = DeepCopyShape(ItemData.TryGetItemSize());
            _rotationAngle = Rect.localEulerAngles.z;

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
            _dragDisposables.Clear();

            if (!_inventoryView.IsAvailablePlaceByCenter(this))
            {
                transform.SetParent(_startParent);
                transform.position = _startPosition;
                _image.color = Color.red;

                BaseItem temp = ItemData;
                temp.Grid = _originalGrid;
                ItemData = temp;
                Rect.localRotation = Quaternion.Euler(0, 0, 0);

                if (_inventoryView.Inventory.TryGetPlacement(ID, out PlacementItem placement))
                {
                    _inventoryView.TryRestorePlacement(placement);
                }
            }
            else
            {
                _inventoryView.TryPlaceItem(this);
            }
            _image.color = Color.white;
        }

        #endregion

        #region PUBLIC_REGION

        public void Initialize(BaseItem itemData, InventoryView inventoryView)
        {
            ItemData = itemData;
            _inventoryView = inventoryView;
            ID = GetInstanceID().ToString();
        }

        #endregion

        #region PRIVATE_REGION

        private void RotateClockwise()
        {
            BaseItem temp = ItemData;
            temp.Grid = RotateShapeClockwise(ItemData.Grid);
            ItemData = temp;

            _rotationAngle -= 90;

            Rect.DORotate(new Vector3(0, 0, _rotationAngle), 0.1f);
            Rect.DOMove(Input.mousePosition, 0.1f);
        }

        private Wrapper<CellType>[] RotateShapeClockwise(Wrapper<CellType>[] shape)
        {
            int oldHeight = shape.Length;
            int oldWidth = shape[0].Values.Length;
            Wrapper<CellType>[] newShape = new Wrapper<CellType>[oldWidth];

            Vector2Int oldCenter = GetItemCenter(shape);
            int newCenterX = oldCenter.y;
            int newCenterY = oldHeight - 1 - oldCenter.x;

            for (int i = 0; i < oldWidth; i++)
            {
                newShape[i] = new Wrapper<CellType> { Values = new CellType[oldHeight] };
                for (int j = 0; j < oldHeight; j++)
                {
                    newShape[i].Values[j] = shape[oldHeight - 1 - j].Values[i];
                    if (i == newCenterX && j == newCenterY)
                    {
                        newShape[i].Values[j] = CellType.Center;
                    }
                    else if (newShape[i].Values[j] == CellType.Center)
                    {
                        newShape[i].Values[j] = CellType.Busy;
                    }
                }
            }
            return newShape;
        }

        private Vector2Int GetItemCenter(Wrapper<CellType>[] shape)
        {
            for (int i = 0; i < shape.Length; i++)
            {
                for (int j = 0; j < shape[i].Values.Length; j++)
                {
                    if (shape[i].Values[j] == CellType.Center)
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }
            return new Vector2Int(shape.Length / 2, shape[0].Values.Length / 2);
        }

        private Wrapper<CellType>[] DeepCopyShape(Wrapper<CellType>[] original) 
            => original.Select(w => new Wrapper<CellType> { Values = (CellType[])w.Values.Clone() }).ToArray();

        #endregion
    }
}