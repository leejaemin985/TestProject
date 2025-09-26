using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;
using InGame.Weapon;
using InGame.Event;

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

        private SimpleKCC cc;

        protected Player player { get; private set; }
        protected PlayerFSM fsm { get; private set; }
        protected Animator latencyInterpolationAnim { get; private set; }
        protected Animator modelAnim { get; private set; }
        protected IWeapon weap { get; private set; }

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

        public bool IsGrounded() => cc.IsGrounded;

        public void Move(Vector3 moveDir, float jumpImpulse = 0)
        {
            if (HasStateAuthority == false) return;
            cc.Move(moveDir, jumpImpulse);
        }

        public void SetLookRotation(Quaternion lookRotation)
        {
            if (HasStateAuthority == false) return;
            cc.SetLookRotation(lookRotation);
        }


        #region Base

        public abstract StateType GetStateType();

        protected abstract StatePriorityType Priority { get; }

        protected virtual void SetInfo(INetworkStruct info) { }


        protected virtual void EnterStateAuthority(int enterTick) { }

        protected virtual void EnterStateShared(int enterTick) { }


        protected virtual void EnterState(int enterTick)
        {
            if (HasStateAuthority) EnterStateAuthority(enterTick);

            EnterStateShared(enterTick);
        }


        protected virtual void ExitStateAuthority() { }

        protected virtual void ExitStateShared() { }

        protected void ExitState()
        {
            if (HasStateAuthority) ExitStateAuthority();
            ExitStateShared();
        }


        protected virtual void OnState() { }

        protected virtual void OnRender() { }

        protected virtual void OnMasterTick() { }


        protected virtual void OnAnimEvent(string param) { }

        protected virtual void OnAnimEvent(AnimationEventData data) { }

        #endregion



        #region IState

        StatePriorityType IState.priority => Priority;

        void IState.SetInfo(StateInfo info) => SetInfo(info);

        void IState.EnterState(int enterTick) => EnterState(enterTick);

        void IState.OnState() => OnState();

        void IState.ExitState() => ExitState();

        void IState.OnRender() => OnRender();

        void IState.OnAnimEvent(string param) => OnAnimEvent(param);

        void IState.OnAnimEvent(AnimationEventData eventData)=> OnAnimEvent(eventData);

        void IState.OnMasterTick() => OnMasterTick();

        #endregion

    }
}