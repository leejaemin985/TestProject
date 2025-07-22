namespace Unit
{
    public interface IState
    {
        PlayerStateBase.StateType GetStateType();

        void EnterState(bool syncMotion);

        void OnState();

        void ExitState();

        void OnRender();

        void OnAnimEvent(string param);
    }
}