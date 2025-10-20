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

        protected Action<PhaseState> reportPhase { get; private set; }

        protected PhaseDirective phaseDirective { get; private set; }


        protected virtual Task<PhaseState> OnEnter(PhaseDirective phaseDirective) => Task.FromResult(PhaseState.Wait);

        protected virtual Task<PhaseState> OnExit() => Task.FromResult(PhaseState.Exit);


        #region IClientPhase
        FlowPhase IClientPhase.phaseType => phaseType;

        Task<PhaseState> IClientPhase.OnEnter(PhaseDirective phaseDirective)
        {
            this.phaseDirective = phaseDirective;
            return OnEnter(phaseDirective);
        }

        Task<PhaseState> IClientPhase.OnExit() => OnExit();
        #endregion


        public virtual void Initialize(Action<PhaseState> reportAction)
        {
            this.reportPhase = reportAction;
        }

    }
}