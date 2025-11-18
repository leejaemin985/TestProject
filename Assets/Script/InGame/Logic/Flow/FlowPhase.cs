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
        DeathScene,
        End,

        Count
    }

    public enum PhaseState
    {
        None = -1,

        Init,
        Complete,
        Exit,
        Error
    }

    public struct PhaseReport : INetworkStruct
    {
        public PlayerRef userRef;
        public FlowPhase phaseType;
        public PhaseState phaseState;

        public bool IsValid => phaseType != FlowPhase.None;
    }

    public struct PhaseDirective : INetworkStruct
    {
        public FlowPhase phaseType;
        public int startTick;
    }

}