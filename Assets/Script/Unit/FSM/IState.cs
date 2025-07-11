namespace Unit
{
    public interface IState
    {
        bool CanEnter();

        void EnterState();

        void OnState();

        void ExitState();

        void OnRender();
    }
}