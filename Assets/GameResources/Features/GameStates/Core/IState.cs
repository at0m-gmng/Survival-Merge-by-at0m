namespace GameResources.Features.GameStates.Core
{
    public interface IPayloadState<TPayload> : IExitableState
    {
        public void Enter(TPayload payload);
    }
    
    public interface IState : IExitableState
    {
        public void Enter();
    }
    
    public interface IExitableState
    {
        public void Exit();
    }
}