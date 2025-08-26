using Fusion;

namespace InGame.Logic.Flow
{
    public enum FlowPhase
    {
        Init = 0,

        SessionSpawn,
        UnitSpawn,
        Warmup,
        InBattle,
        End,

        Count
    }

    public struct PhaseReport : INetworkStruct
    {
        public PlayerRef userRef;
        public FlowPhase phase;
    }

    public struct PhaseDirective : INetworkStruct
    {
        public FlowPhase phase;
        public int startTick;
    }

}