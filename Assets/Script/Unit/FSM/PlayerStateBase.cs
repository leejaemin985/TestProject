using Fusion.Addons.SimpleKCC;
using System.Data;
using UnityEngine;

namespace Unit
{
    public abstract class PlayerStateBase : MonoBehaviour, IState
    {
        public enum StateType
        {
            Move,
            Attack,
            Hit
        }

        public abstract StateType stateType { get; }

        protected PlayerFSM fsm;
        protected KCC cc => fsm.cc;
        protected Animator anim => fsm.anim;

        [SerializeField] protected string animState;

        public void InjectFSM(PlayerFSM fsm)
        {
            this.fsm = fsm;
        }

        protected virtual void EnterState()
        {
            fsm.RPC_SyncState(stateType);
        }

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