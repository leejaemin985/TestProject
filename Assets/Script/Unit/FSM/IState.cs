using Fusion;

namespace Unit
{
    public interface IState
    {
        PlayerStateBase.StateType GetStateType();

        void SetInfo(INetworkStruct info);

        void EnterState(bool syncMotion);

        void OnState();

        void ExitState();

        void OnRender();

        void OnMasterTick();

        void OnAnimEvent(string param);
    }
}