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

        protected PhaseReport CreatePhaseReport(PhaseState state)
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


        protected virtual Task<PhaseReport> OnEnter(PhaseDirective phaseDirective) => Task.FromResult(CreatePhaseReport(PhaseState.Init));

        protected virtual Task<PhaseReport> OnExit() => Task.FromResult(CreatePhaseReport(PhaseState.Exit));

        protected virtual PhaseState OnTick(float deltaTime) => PhaseState.Wait;

        #region IClientPhase

        FlowPhase IClientPhase.phaseType => phaseType;

        Task<PhaseReport> IClientPhase.OnEnter(PhaseDirective phaseDirective)
        {
            this.phaseDirective = phaseDirective;
            return OnEnter(phaseDirective);
        }

        Task<PhaseReport> IClientPhase.OnExit() => OnExit();

        PhaseState IClientPhase.OnTick(float deltaTime) => OnTick(deltaTime);

        #endregion


        public virtual void Initialize() { }

    }
}