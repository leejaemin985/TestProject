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

        public virtual Task OnEnter() => Task.CompletedTask;

        public virtual Task OnExit() => Task.CompletedTask;

        public virtual void Tick(int tick, float dt) { }

        #region IClientPhase

        FlowPhase IClientPhase.phaseType => phaseType;

        Task IClientPhase.OnEnter() => OnEnter();

        Task IClientPhase.OnExit() => OnExit();

        void IClientPhase.Tick(int tick, float dt) => Tick(tick, dt);

        #endregion


        protected Action phaseDoneListener { get; private set; }

        public void Initialize(Action phaseDoneListener)
        {
            this.phaseDoneListener = phaseDoneListener;
        }
    }
}