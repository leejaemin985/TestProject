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
            Roar,
            Hit,
            Died
        }

        public enum StatePriorityType
        {
            Free = 10,

            Event = 20,

            Override = 30,

            Terminal = 40
        }

        protected Player player;
        protected PlayerFSM fsm;
        protected SimpleKCC cc;
        protected Animator latencyInterpolationAnim;
        protected Animator modelAnim;
        protected IWeapon weap;

        #region StatusDefinition
        public virtual bool HasSuperArmor => false;
        #endregion

        public virtual void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, IWeapon weap)
        {
            this.player = player;
            this.fsm = fsm;
            this.cc = cc;
            this.modelAnim = modelAnim;
            this.latencyInterpolationAnim = latencyInterpolationAnim;
            this.weap = weap;
        }

        protected void PlayAnim(string stateName, float fixedTransitionDuration, int enterTick)
        {
            if (Runner.IsSharedModeMasterClient)
            {
                float latency = (Runner.Tick - enterTick) * Runner.DeltaTime;
                latencyInterpolationAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, latency);
            }
            modelAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, 0);
        }


        protected void PlayAnim(PlayerFSM.TransitionTypeInFSM transitionType, StatePriorityType statePriorityType, string stateName, float fixedTransitionDuration, bool sync)
        {
            if (!HasStateAuthority || sync == false)
                modelAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
            else
                RPC_AnimCrossFadeInFixedTime(transitionType, statePriorityType, stateName, fixedTransitionDuration, Runner.Tick, fsm.systemSeq);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnimCrossFadeInFixedTime(PlayerFSM.TransitionTypeInFSM transitionType, StatePriorityType statePriorityType, string stateName, float fixedTransitionDuration, int tick, int seq)
        {
            if (fsm.CanSetState(transitionType, statePriorityType, seq) == false) return;

            if (Runner.IsSharedModeMasterClient)
            {
                float latency = (Runner.Tick - tick) * Runner.DeltaTime;
                latencyInterpolationAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, latency);
            }

            modelAnim.CrossFadeInFixedTime(stateName, fixedTransitionDuration, 0, 0);
        }


        #region Base

        public abstract StateType GetStateType();

        protected abstract StatePriorityType Priority { get; }

        protected virtual void SetInfo(INetworkStruct info) { }

        protected virtual void EnterState(int enterTick) { }

        protected virtual void EnterState(PlayerFSM.TransitionTypeInFSM transitionType, bool sync = true) { }

        protected virtual void OnState() { }

        protected virtual void ExitState() { }

        protected virtual void OnEnterRender() { }

        protected virtual void OnRender() { }

        protected virtual void OnExitRender() { }

        protected virtual void OnMasterTick() { }

        protected virtual void OnAnimEvent(string param) { }

        #endregion


        #region IState

        StatePriorityType IState.priority => Priority;

        void IState.SetInfo(StateInfo info) => SetInfo(info);

        void IState.EnterState(int enterTick) => EnterState(enterTick);

        void IState.EnterState(PlayerFSM.TransitionTypeInFSM transitionType, bool sync) => EnterState(transitionType, sync);

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