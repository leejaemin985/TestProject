using System;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;

namespace InGame.Logic.Flow
{
    public abstract class ClientPhaseBase : MonoBehaviour, IClientPhase
    {
        protected NetworkRunner runner => GameNetworkManager.Instance.runner;

        public abstract FlowPhase phaseType { get; }

        protected PhaseReport GetValidPhaseReport(PhaseState state)
        {
            return new()
            {
                userRef = runner.LocalPlayer,
                phaseType = phaseType,
                phaseState = state
            };
        }

        protected PhaseReport currentPhaseReport { get; set; }

        protected PhaseDirective phaseDirective { get; private set; }


        public virtual Task<PhaseReport> OnEnter(PhaseDirective phaseDirective) => Task.FromResult(GetValidPhaseReport(PhaseState.Init));

        public virtual Task<PhaseReport> OnExit() => Task.FromResult(GetValidPhaseReport(PhaseState.Exit));


        #region IClientPhase

        FlowPhase IClientPhase.phaseType => phaseType;

        Task<PhaseReport> IClientPhase.OnEnter(PhaseDirective phaseDirective)
        {
            this.phaseDirective = phaseDirective;
            return OnEnter(phaseDirective);
        }

        Task<PhaseReport> IClientPhase.OnExit() => OnExit();

        #endregion


        public virtual void Initialize() { }

    }
}