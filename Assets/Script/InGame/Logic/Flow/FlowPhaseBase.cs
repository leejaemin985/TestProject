using System;
using System.Threading.Tasks;
using Fusion;

namespace InGame.Logic.Flow
{
    public abstract class FlowPhaseBase : NetworkBehaviour, IFlowPhase
    {
        protected abstract FlowPhase PhaseType { get; }

        protected Action phaseEnterListener { get; private set; }
        protected Action phaseDoneListener { get; private set; }

        protected virtual Task Initialize(Action phaseEnterListener, Action phaseDoneListener)
        {
            this.phaseEnterListener = phaseEnterListener;
            this.phaseDoneListener = phaseDoneListener;

            return Task.CompletedTask;
        }

        protected virtual Task EnterPhase()
        {
            phaseEnterListener?.Invoke();
            return Task.CompletedTask;
        }

        protected virtual Task ExitPhase()
        {
            phaseDoneListener?.Invoke();
            return Task.CompletedTask;
        }

        protected virtual void OnPhase() { }


        FlowPhase IFlowPhase.PhaseType => PhaseType;

        Task IFlowPhase.Initialize(Action phaseEnterListener, Action phaseDoneListener) => Initialize(phaseEnterListener, phaseDoneListener);

        Task IFlowPhase.EnterPhase() => EnterPhase();

        void IFlowPhase.OnPhase() => OnPhase();

        Task IFlowPhase.ExitPhase() => ExitPhase();
    }
}