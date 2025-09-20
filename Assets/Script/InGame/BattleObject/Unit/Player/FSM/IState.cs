using Fusion;

namespace Unit
{
    public interface IState
    {
        PlayerStateBase.StateType GetStateType();

        PlayerStateBase.StatePriorityType priority { get; }

        #region Status
        
        bool HasSuperArmor { get; }

        #endregion

        void SetInfo(INetworkStruct info);

        void EnterState(PlayerFSM.TransitionType transitionType, bool syncMotion);

        void OnState();

        void ExitState();

        void OnEnterRender();

        void OnRender();

        void OnExitRender();

        void OnMasterTick();

        void OnAnimEvent(string param);
    }
}