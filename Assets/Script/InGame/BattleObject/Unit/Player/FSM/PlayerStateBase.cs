using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;
using InGame.Weapon;

namespace Unit
{
    public abstract class PlayerStateBase : NetworkBehaviour, IState
    {
        public enum StateType
        {
            Move = 0,
            Sprint,
            Jump,
            Land,
            Attack,
            Defense,
            Parring,
            Hit,
            Died
        }

        protected Player player;
        protected PlayerFSM fsm;
        protected SimpleKCC cc;
        protected Animator latencyInterpolationAnim;
        protected Animator modelAnim;
        protected IWeapon weap;

        public virtual void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, IWeapon weap)
        {
            this.player = player;
            this.fsm = fsm;
            this.cc = cc;
            this.modelAnim = modelAnim;
            this.latencyInterpolationAnim = latencyInterpolationAnim;
            this.weap = weap;
        }


        protected void PlayAnim(string stateName, float fixedTransitionDuration, bool sync)
        {
            if (!HasStateAuthority || sync == false)
                modelAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
            else
                RPC_AnimCrossFadeInFixedTime(stateName, fixedTransitionDuration, Runner.Tick);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnimCrossFadeInFixedTime(string stateName, float fixedTransitionDuration, int tick)
        {
            if (fsm.isStateLockActive) return;

            if (Runner.IsSharedModeMasterClient)
            {
                float latency = (Runner.Tick - tick) * Runner.DeltaTime;
                latencyInterpolationAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, latency);
            }

            modelAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, 0);
        }

        #region Base

        public abstract StateType GetStateType();

        public int priority => (int)GetStateType();

        protected virtual void SetInfo(INetworkStruct info) { }

        protected virtual void EnterState(bool sync = true) { }

        protected virtual void OnState() { }

        protected virtual void ExitState() { }

        protected virtual void OnEnterRender() { }

        protected virtual void OnRender() { }

        protected virtual void OnExitRender() { }

        protected virtual void OnMasterTick() { }

        protected virtual void OnAnimEvent(string param) { }

        #endregion


        #region IState

        void IState.SetInfo(INetworkStruct info) => SetInfo(info);

        void IState.EnterState(bool sync) => EnterState(sync);

        void IState.OnState() => OnState();

        void IState.ExitState() => ExitState();

        void IState.OnEnterRender() => OnEnterRender();

        void IState.OnRender() => OnRender();

        void IState.OnExitRender() => OnExitRender();

        void IState.OnAnimEvent(string param) => OnAnimEvent(param);

        void IState.OnMasterTick() => OnMasterTick();

        #endregion

    }
}