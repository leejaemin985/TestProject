using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Data;
using UnityEngine;

namespace Unit
{
    public abstract class PlayerStateBase : NetworkBehaviour, IState
    {
        public enum StateType
        {
            Move,
            Attack,
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

        protected virtual void EnterState(bool sync = true) { }

        protected virtual void OnState() { }

        protected virtual void ExitState() { }

        protected virtual void OnRender() { }

        protected virtual void OnAnimEvent(string param) { }

        private float remainingCorrectionTime = 0f;
        private const float ANIM_CORRECTION_WINDOW = .05f;

        protected void PlayAnim(string stateName, float fixedTransitionDuration, bool sync)
        {
            //if (!HasStateAuthority || sync == false)
            //{
            //    anim.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
            //}
            //else
            //{
            //    RPC_AnimCrossFadeInFixedTime(stateName, fixedTransitionDuration, Runner.Tick);
            //}
            RPC_AnimCrossFadeInFixedTime(stateName, fixedTransitionDuration, Runner.Tick);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnimCrossFadeInFixedTime(string stateName, float fixedTransitionDuration, int tick)
        {
            //float latency = (Runner.Tick - tick) * Runner.DeltaTime;
            //anim.speed = 1f + (latency / ANIM_CORRECTION_WINDOW);
            //remainingCorrectionTime = ANIM_CORRECTION_WINDOW;

            anim.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
            //anim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, latency);
        }

        public override void Render()
        {
            if (remainingCorrectionTime > 0f)
            {
                remainingCorrectionTime -= Time.deltaTime;
                if (remainingCorrectionTime <= 0f)
                {
                    anim.speed = 1f;
                }
            }
        }

        void IState.EnterState(bool sync) => EnterState(sync);

        void IState.OnState() => OnState();

        void IState.ExitState() => ExitState();

        void IState.OnRender() => OnRender();

        void IState.OnAnimEvent(string param) => OnAnimEvent(param);
    }
}