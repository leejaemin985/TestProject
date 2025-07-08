using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace Unit
{
    public class PlayerFSM : MonoBehaviour, IMachineState
    {
        [SerializeField] private PlayerStateBase[] stateArray = default;

        private KCC kcc;
        private Animator animator;

        public KCC cc => kcc;
        public Animator anim => animator;

        private IState currentState;

        public void Initialized(KCC cc, Animator anim)
        {
            kcc = cc;
            animator = anim;

            foreach (var state in stateArray)
            {
                state.InjectFSM(this);
            }
        }

        void IMachineState.SetState<T>()
        {
            for (int index = 0, max = stateArray.Length; index < max; ++index)
            {
                if (stateArray[index] is T state)
                {
                    if (state.CanEnter() == false) return;

                    currentState?.ExitState();
                    currentState = state;
                    currentState.EnterState();
                }
            }
        }

        void IMachineState.SetForceState<T>()
        {
            for (int index = 0, max = stateArray.Length; index < max; ++index)
            {
                if (stateArray[index] is T state)
                {
                    currentState?.ExitState();
                    currentState = state;
                    currentState.EnterState();
                }
            }
        }
    }
}