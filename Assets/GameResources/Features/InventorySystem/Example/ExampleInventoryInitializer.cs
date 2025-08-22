namespace GameResources.Features.InventorySystem.Example
{
    using Data;
    using UniRx;
    using UnityEngine;

    public sealed class ExampleInventoryInitializer : MonoBehaviour
    {
        [SerializeField] private bool _isLoad = false;
        [SerializeField] private InventoryView _inventoryView = default;
        [SerializeField] private Transform _itemsParent = default;

        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            _inventoryView.Initialized
                .Where(v => v)
                .Take(1)
                .Subscribe(_ =>
                {
                    if (_isLoad)
                    {
                        foreach (ItemData data in _inventoryView.Inventory.BaseItems)
                        {
                            ItemView createdItemView = Instantiate(data.UIPrefab, _inventoryView.ItemParent);
                            createdItemView.Initialize(data, _inventoryView);
                            _inventoryView.TryPlaceItem(createdItemView);
                        }
                    }
                    else
                    {
                        
                        foreach (ItemData data in _inventoryView.Inventory.BaseItems)
                        {
                            ItemView createdItemView = Instantiate(data.UIPrefab, _itemsParent);
                            createdItemView.Initialize(data, _inventoryView);
                        }
                    }
                })
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}