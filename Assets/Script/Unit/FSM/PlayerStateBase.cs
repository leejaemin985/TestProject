using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace Unit
{
    public abstract class PlayerStateBase : MonoBehaviour, IState
    {
        [SerializeField] private string stateName;
        public string StateName => stateName;

        protected PlayerFSM fsm;
        protected KCC cc => fsm.cc;
        protected Animator anim => fsm.anim;

        public void InjectFSM(PlayerFSM fsm)
        {
            this.fsm = fsm;
        }

        protected virtual bool CanEnter() { return true; }

        protected virtual void EnterState() { }

        protected virtual void OnState() { }

        protected virtual void ExitState() { }

        protected virtual void OnRender() { }

        protected virtual void OnAnimEvent(string param) { }

        bool IState.CanEnter() => CanEnter();

        void IState.EnterState() => EnterState();

        void IState.OnState() => OnState();

        void IState.ExitState() => ExitState();

        void IState.OnRender() => OnRender();

        void IState.OnAnimEvent(string param) => OnAnimEvent(param);
    }
}