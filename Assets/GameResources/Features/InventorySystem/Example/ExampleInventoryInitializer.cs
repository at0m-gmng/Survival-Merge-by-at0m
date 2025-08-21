namespace GameResources.Features.InventorySystem.Example
{
    using EditorGridDrawled;
    using Data;
    using UniRx;
    using UnityEngine;

    public sealed class ExampleInventoryInitializer : MonoBehaviour
    {
        [SerializeField] private InventoryView _inventoryView = default;

        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            _inventoryView.Initialized
                .Where(v => v)
                .Take(1)
                .Subscribe(_ =>
                {
                    foreach (ItemData data in _inventoryView.Inventory.BaseItems)
                    {
                        ItemView createdItemView = Instantiate(data.UIPrefab, _inventoryView.ItemParent);
                        createdItemView.Initialize(data);
                        _inventoryView.TryPlaceItem(createdItemView);
                    }
                })
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}