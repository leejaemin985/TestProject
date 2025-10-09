using Fusion;

namespace InGame.Logic.Flow
{
    public enum FlowPhase
    {
        Init = 0,

        SessionInit,
        UnitSpawn,
        Intro,
        InBattle,
        End,

        Count
    }

    //public struct PhaseReport : INetworkStruct
    //{
    //    public PlayerRef userRef;
    //    public FlowPhase phase;
    //    public int tick;
    //}

    //public struct PhaseDirective : INetworkStruct
    //{
    //    public PlayerRef userRef;
    //    public FlowPhase phase;
    //    public int tick;
    //}


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