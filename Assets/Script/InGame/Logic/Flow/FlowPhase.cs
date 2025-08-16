using Fusion;

namespace InGame.Logic.Flow
{
    public enum FlowPhase
    {
        Init = 0,
        SessionSpawn = 1,
        UnitInfoSpawn = 2,
        UnitSpawn = 3,
        Bind = 4,
        WarmUp = 5,
        InBattle = 6
    }

    public struct PhaseReport : INetworkStruct
    {
        public PlayerRef userRef;
        public FlowPhase phase;
    }

    public struct PhaseDirective : INetworkStruct
    {
        public FlowPhase phase;

    }

}