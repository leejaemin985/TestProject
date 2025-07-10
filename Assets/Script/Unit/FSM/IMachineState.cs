namespace Unit
{
    public interface IMachineState
    {
        void SetState<T>() where T : class, IState;

        void SetForceState<T>() where T : class, IState;

        void UpdateTick(InputData newInput, int tick, float deltaTime);
    }
}