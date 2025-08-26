using Fusion;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace InGame.Logic.Flow
{
    public abstract class ClientPhaseBase : MonoBehaviour, IClientPhase
    {
        protected NetworkRunner runner => GameNetworkManager.Instance.runner;

        public abstract FlowPhase phaseType { get; }
        protected PhaseDirective phaseDirective { get; private set; }

        public virtual Task OnEnter()
        {
            phaseDoneListener?.Invoke();
            return Task.CompletedTask;
        }

        public virtual Task OnExit() => Task.CompletedTask;

        #region IClientPhase

        FlowPhase IClientPhase.phaseType => phaseType;

        Task IClientPhase.OnEnter(PhaseDirective phaseDirective)
        {
            this.phaseDirective = phaseDirective;
            return OnEnter();
        }

        Task IClientPhase.OnExit() => OnExit();

        #endregion


        protected Action phaseDoneListener { get; private set; }

        public void Initialize(Action phaseDoneListener)
        {
            this.phaseDoneListener = phaseDoneListener;
        }
    }
}