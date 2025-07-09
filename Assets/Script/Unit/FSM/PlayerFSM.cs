using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace Unit
{
    public class PlayerFSM : NetworkBehaviour, IMachineState
    {
        [SerializeField] private PlayerStateBase[] stateArray = default;

        private Player playerUnit;
        private SimpleKCC kcc;
        private PlayerFSMAnimController animator;
        [SerializeField] private float animatorTransitionDuration = 4f / 60f;

        public Player player => playerUnit;
        public SimpleKCC cc => kcc;
        public PlayerFSMAnimController anim => animator;
        public float animTransitionDuration => animatorTransitionDuration;

        public int cachedTick { get; private set; }

        public InputInterpreter input;

        private IState currentState;

        #region Networked

        [Networked]
        public Vector3 moveAnimDir { get; set; }

        [Networked]
        public float runWeight { get; set; }

        #endregion

        public void Initialized(Player player, SimpleKCC cc, PlayerFSMAnimController anim)
        {
            this.playerUnit = player;
            kcc = cc;
            animator = anim;
            input = new();

            foreach (var state in stateArray)
            {
                state.InjectFSM(this);
            }

            SetState<PlayerMovementState>();
        }

        public void SetState<T>() where T : class, IState
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

        public void SetForceState<T>() where T : class, IState
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

        public override void FixedUpdateNetwork()
        {
            cachedTick = Runner.Tick;

            currentState?.OnState();
        }
    }
}