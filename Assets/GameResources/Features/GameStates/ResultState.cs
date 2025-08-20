namespace GameResources.Features.GameStates
{
    using Core;
    using UISystem;
    using UISystem.SO;
    using UnityEngine;

    public sealed class ResultState : IState
    {
        public ResultState(IGameStateMachine gameStateMachine, IUISystem uiSystem)
        {
            _gameStateMachine = gameStateMachine;
            _uiSystem = uiSystem;
        }
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IUISystem _uiSystem;
        
        private ResultWindow _resultWindow = default;

        public void Enter()
        {
            Debug.Log($"Enter {nameof(ResultState)}");
            _uiSystem.TryGetWindow(UIWindowID.Result, out _resultWindow);

            _resultWindow.ButtonRestart.onClick.AddListener(OnRestartButtonClicked);
            _resultWindow.ButtonMenu.onClick.AddListener(OnButtonMenuClicked);
            
            _uiSystem.ShowWindow(UIWindowID.Result);
        }

        public void Exit()
        {
            _uiSystem.HideWindow(UIWindowID.Result);
            Debug.Log($"Exit {nameof(ResultState)}");
        }

        private void OnRestartButtonClicked() => _gameStateMachine.Enter<GameState>();

        private void OnButtonMenuClicked() => _gameStateMachine.Enter<MenuState>();
    }
}