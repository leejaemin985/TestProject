using Fusion;
using Fusion.Addons.SimpleKCC;

using UnityEngine;
using UnityEngine.Animations;

namespace Unit
{
    public abstract class PlayerStateBase : NetworkBehaviour, IState
    {
        public enum StateType
        {
            Move = 0,
            Jump,
            Attack,
            Defense,
            Parring,
            Hit
        }

        protected Player player;
        protected PlayerFSM fsm;
        protected SimpleKCC cc;
        protected Animator anim;
        protected Katana weap;

        public virtual void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator anim, Katana weap)
        {
            this.player = player;
            this.fsm = fsm;
            this.cc = cc;
            this.anim = anim;
            this.weap = weap;
        }

        public abstract StateType GetStateType();

        protected virtual void SetInfo(INetworkStruct info) { }

        protected virtual void EnterState(bool sync = true) { }

        protected virtual void OnState() { }

        protected virtual void ExitState() { }

        protected virtual void OnRender() { }

        protected virtual void OnMasterTick() { }

        protected virtual void OnAnimEvent(string param) { }

        protected void PlayAnim(string stateName, float fixedTransitionDuration, bool sync)
        {
            if (!HasStateAuthority || sync == false)
                anim.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
            else
                RPC_AnimCrossFadeInFixedTime(stateName, fixedTransitionDuration, Runner.Tick);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnimCrossFadeInFixedTime(string stateName, float fixedTransitionDuration, int tick)
        {
            if (fsm.isStateLockActive) return;

            float latency = (Runner.Tick - tick) * Runner.DeltaTime;
            anim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, latency);
        }


        void IState.SetInfo(INetworkStruct info) => SetInfo(info);

        void IState.EnterState(bool sync) => EnterState(sync);

        void IState.OnState() => OnState();

        void IState.ExitState() => ExitState();

        void IState.OnRender() => OnRender();

        void IState.OnAnimEvent(string param) => OnAnimEvent(param);

        void IState.OnMasterTick() => OnMasterTick();
    }
}