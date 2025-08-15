using Fusion;

namespace InGame.Logic.Flow
{
    public struct PhaseReport : INetworkStruct
    {
        public PlayerRef userRef;
        public FlowPhase phase;
    }
}