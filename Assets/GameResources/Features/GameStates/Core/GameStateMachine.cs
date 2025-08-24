namespace GameResources.Features.GameStates.Core
{
    using System;
    using System.Collections.Generic;

    public sealed class GameStateMachine : IGameStateMachine
    {
        private IExitableState _activeState = default;
        private Dictionary<Type,IExitableState> _states = new Dictionary<Type, IExitableState>();
        
        public void Enter<TState>() where TState : class, IState
        {
            IState state = ChangeState<TState>();
            state.Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
        {
            TState state = ChangeState<TState>();
            state.Enter(payload);
        }
        
        public void RegisterState<TState>(TState state) where TState : class, IExitableState 
            => _states[state.GetType()] = state;

        public IExitableState GetState(Type stateType) 
            => _states.TryGetValue(stateType, out IExitableState state) ? state : null;

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            _activeState?.Exit();
            
            TState state = GetState<TState>();
            _activeState = state;
            
            return state;
        }
        
        private TState GetState<TState>() where TState : class, IExitableState 
            => _states[typeof(TState)] as TState;
    }

    public interface IGameStateMachine
    {
        public void Enter<TState>() where TState : class, IState;
        
        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>;
        
        public void RegisterState<TState>(TState state) where TState : class, IExitableState;
    }
}