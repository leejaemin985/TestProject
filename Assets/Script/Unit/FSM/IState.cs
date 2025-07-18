namespace Unit
{
    public interface IState
    {
        void EnterState();

        void OnState();

        void ExitState();

        void OnRender();

        void OnAnimEvent(string param);
    }
}