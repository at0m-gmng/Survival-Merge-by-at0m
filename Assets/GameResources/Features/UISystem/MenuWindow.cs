namespace GameResources.Features.UISystem
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class MenuWindow : BaseWindow
    {
        [Header("Buttons")]
        [field: SerializeField] public Button ButtonStart { get; private set; } = default;

        private Sequence _buttonAnimation = null;
        
        private void Awake() => CreateButtonAnimation();

        private void OnDestroy() => _buttonAnimation.Kill();

        public override void Show()
        {
            base.Show();
            _buttonAnimation.Restart();
        }

        public override void Hide()
        {
            _buttonAnimation.Rewind();
            base.Hide();
        }

        private void CreateButtonAnimation()
        {
            _buttonAnimation = DOTween.Sequence();
            _buttonAnimation
                .Append(ButtonStart.transform.DOScale(0.8f, 0.25f).SetEase(Ease.Linear))
                .Append(ButtonStart.transform.DOScale(1.2f, 1f).SetEase(Ease.OutBack))
                ;
            _buttonAnimation.SetAutoKill(false).SetLoops(-1, LoopType.Yoyo);
        }
    }
}