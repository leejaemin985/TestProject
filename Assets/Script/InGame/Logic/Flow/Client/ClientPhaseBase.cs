using System;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;

namespace InGame.Logic.Flow
{
    public abstract class ClientPhaseBase : MonoBehaviour, IClientPhase
    {
        protected static NetworkRunner runner => GameNetworkManager.Instance.runner;

        public abstract FlowPhase phaseType { get; }

        protected PhaseDirective phaseDirective { get; private set; }


        protected virtual Task<PhaseState> OnEnter(PhaseDirective phaseDirective) => Task.FromResult(PhaseState.Init);

        protected virtual Task<PhaseState> OnExit() => Task.FromResult(PhaseState.Exit);

        protected virtual Task<PhaseState> OnPhase() => Task.FromResult(PhaseState.Complete);

        #region IClientPhase
        FlowPhase IClientPhase.phaseType => phaseType;

        Task<PhaseState> IClientPhase.OnEnter(PhaseDirective phaseDirective)
        {
            this.phaseDirective = phaseDirective;
            return OnEnter(phaseDirective);
        }

        Task<PhaseState> IClientPhase.OnExit() => OnExit();

        Task<PhaseState> IClientPhase.OnPhase() => OnPhase();

        #endregion


        public virtual void Initialize() { }

    }
}