using Fusion;

namespace InGame.Logic.Flow
{
    public enum FlowPhase
    {
        None = -1,

        Init = 0,
        SessionInit,
        UnitSpawn,
        Intro,
        InBattle,
        End,

        Count
    }

    public enum PhaseState
    {
        None = -1,

        Init,
        Run,
        Wait,
        Exit,
        Error
    }

    public struct PhaseReport : INetworkStruct
    {
        public PlayerRef userRef;
        public FlowPhase phaseType;
        public PhaseState phaseState;

        public bool IsValid =>
            !userRef.IsNone &&
            phaseType != FlowPhase.None &&
            phaseState != PhaseState.None;
    }

    public struct PhaseDirective : INetworkStruct
    {
        public FlowPhase phaseType;
        public int startTick;
    }

}