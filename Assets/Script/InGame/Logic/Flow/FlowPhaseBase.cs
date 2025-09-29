using Fusion;

namespace InGame.Logic.Flow
{
    public abstract class FlowPhaseBase : NetworkBehaviour, IFlowPhase
    {
        protected abstract FlowPhase PhaseType { get; }

        protected virtual void EnterPhase() { }

        protected virtual void OnPhase() { }


        FlowPhase IFlowPhase.PhaseType => PhaseType;

        void IFlowPhase.EnterPhase() => EnterPhase();

        void IFlowPhase.OnPhase() => OnPhase();
    }
}