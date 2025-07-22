namespace Unit
{
    public interface IMachineState
    {
        void SetState<T>(bool sync = true) where T : class, IState;
    }
}