using Fusion;
using InGame.Event;

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

        void OnRender();

        void OnMasterTick();

        void OnAnimEvent(AnimationEventData eventData);
    }
}