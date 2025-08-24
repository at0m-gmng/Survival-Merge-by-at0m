namespace GameResources.Features.GameStates
{
    using Core;
    using UISystem;
    using UnityEngine;

    public sealed class BootstrapState : IState
    {
        public BootstrapState(IGameStateMachine gameStateMachine, IUISystem uiSystem)
        {
            _gameStateMachine = gameStateMachine;
            _uiSystem = uiSystem;
        }
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IUISystem _uiSystem;

        public async void Enter()
        {
            Debug.Log($"Enter {nameof(BootstrapState)}");
            await _uiSystem.Initialize();

            _gameStateMachine.Enter<MenuState>();
        }

        public void Exit() => Debug.Log($"Exit {nameof(BootstrapState)}");
    }
}