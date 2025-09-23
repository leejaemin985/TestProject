using System.Runtime.InteropServices;
using Fusion;

namespace Unit
{
    public enum TransitionType
    {
        Request,
        System,
    }

    public struct StateTransitionData : INetworkStruct
    {
        public TransitionType transitionType;
        public int systemSeq;

        public PlayerStateBase.StateType stateType;
        public StateInfo stateInfo;
        public int tick;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct StateInfo : INetworkStruct
    {
        [FieldOffset(0)] public MoveInfo moveInfo;
        [FieldOffset(0)] public HitInfo hitInfo;
        [FieldOffset(0)] public AttackInfo attackInfo;
    }
}