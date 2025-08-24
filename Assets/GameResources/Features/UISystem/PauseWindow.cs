namespace GameResources.Features.UISystem
{
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class PauseWindow : BaseWindow
    {
        [Header("Buttons")]
        [field: SerializeField] public Button ButtonContinue { get; set; } = default;
        [field: SerializeField] public Button ButtonMenu { get; set; } = default;
    }
}