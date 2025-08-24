namespace GameResources.Features.InventorySystem.Example
{
    using Cysharp.Threading.Tasks;
    using Data;
    using MergeData;
    using UniRx;
    using UnityEngine;

    public sealed class ExampleInventoryInitializer : MonoBehaviour
    {
        [SerializeField] private bool _isLoad = false;
        [SerializeField] private InventoryView _inventoryView = default;
        [SerializeField] private MergeData _mergeData = default;
        [SerializeField] private Transform _itemsParent = default;

        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            _inventoryView.Initialized
                .Where(v => v)
                .Take(1)
                .Subscribe(async _ =>
                {
                    if (_isLoad)
                    {
                        await UniTask.Delay(50);
                        foreach (ItemData data in _inventoryView.Inventory.BaseItems)
                        {
                            ItemView createdItemView = Instantiate(data.Item.UIPrefab, _itemsParent);
                            createdItemView.Initialize(data.Item, _mergeData, _inventoryView);
                            _inventoryView.TryAutoPlaceItem(createdItemView);
                        }
                    }
                    
                    foreach (ItemData data in _inventoryView.Inventory.BaseItems)
                    {
                        ItemView createdItemView = Instantiate(data.Item.UIPrefab, _itemsParent);
                        createdItemView.Initialize(data.Item,_mergeData, _inventoryView);
                    }
                })
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}