namespace InGame.Logic.Flow
{
    interface IFlowPhase
    {
        FlowPhase PhaseType { get; }

        void EnterPhase();

        void OnPhase();
    }
}