namespace GameResources.Features.EntryPoint
{
    using System.Collections.Generic;
    using GameStates;
    using GameStates.Core;
    using UnityEngine;
    using Zenject;

    public sealed class EntryPoint : MonoBehaviour
    {
        [Inject]
        private void Construct(IGameStateMachine gameStateMachine, List<IState> states)
        {
            _gameStateMachine = gameStateMachine;
            _states = states;
        }
        private IGameStateMachine _gameStateMachine;
        private List<IState> _states;

        private void Start()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _gameStateMachine.RegisterState(_states[i]);
            }            
            _gameStateMachine.Enter<BootstrapState>();
        }
    }
}