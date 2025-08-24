namespace GameResources.Features.UISystem
{
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class ResultWindow : BaseWindow
    {
        [Header("Buttons")]
        [field: SerializeField] public Button ButtonRestart { get; set; } = default;
        [field: SerializeField] public Button ButtonMenu { get; set; } = default;
    }
}