namespace Unit
{
    public interface IMachineState
    {
        void SetState<T>() where T : class, IState;
    }
}