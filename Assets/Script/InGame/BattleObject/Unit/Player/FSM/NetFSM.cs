using Fusion;

namespace Unit
{
    public enum TransitionType
    {
        Request,
        System
    }
    
    public struct NetFSM : INetworkStruct
    {
        public PlayerStateBase.StateType stateType;
        public TransitionType transitionType;
        public int seq;
    }
}