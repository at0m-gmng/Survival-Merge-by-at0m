namespace GameResources.Features.UISystem
{
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class GameWindow : BaseWindow
    {
        [Header("Buttons")]
        [field: SerializeField] public Button ButtonPause { get; private set; } = default;
    }
}