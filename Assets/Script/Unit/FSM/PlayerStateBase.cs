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

        protected PlayerFSM fsm;
        protected SimpleKCC cc;
        protected Animator anim;

        public virtual void Initialize(PlayerFSM fsm, SimpleKCC cc, Animator anim)
        {
            this.fsm = fsm;
            this.cc = cc;
            this.anim = anim;
        }

        public abstract StateType GetStateType();

        protected virtual void EnterState() { }

        protected virtual void OnState() { }

        protected virtual void ExitState() { }

        protected virtual void OnRender() { }

        protected virtual void OnAnimEvent(string param) { }


        void IState.EnterState() => EnterState();

        void IState.OnState() => OnState();

        void IState.ExitState() => ExitState();

        void IState.OnRender() => OnRender();

        void IState.OnAnimEvent(string param) => OnAnimEvent(param);
    }
}