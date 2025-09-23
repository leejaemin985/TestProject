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

        void SetInfo(StateInfo info);

        void EnterState(int enterTick);

        void OnState();

        void ExitState();

        void OnEnterRender();

        void OnRender();

        void OnExitRender();

        void OnMasterTick();

        void OnAnimEvent(string param);
    }
}