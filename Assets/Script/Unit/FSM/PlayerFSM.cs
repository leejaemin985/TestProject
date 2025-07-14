using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using UnityEngine;

namespace Unit
{
    public class PlayerFSM : MonoBehaviour, IMachineState
    {
        public const string RESOURCES_PATH = "Prefab/PlayerFSM";

        [SerializeField] private PlayerStateBase[] stateArray = default;

        private Player playerUnit;
        private SimpleKCC kcc;
        private Animator animator;
        private Katana playerWeapon;


        public Player player => playerUnit;
        public SimpleKCC cc => kcc;
        public Animator anim => animator;
        public Katana playerWeap => playerWeapon;


        public bool HasAuthority => player.HasStateAuthority;
        public int cachedTick { get; private set; }
        public float deltaTime { get; private set; }
        public float tickRate => 1f / deltaTime;

        public InputInterpreter input;

        private IState currentState;

        private Action<string, int, float> rpcRunMotion;

        #region Networked

        public Vector3 moveAnimDir
        {
            get { return playerUnit.moveAnimDir; }
            set { playerUnit.moveAnimDir = value; }
        }

        public float runWeight
        {
            get { return playerUnit.runWeight; }
            set { playerUnit.runWeight = value; }
        }

        #endregion

        public void Initialized(Player player, SimpleKCC cc, Animator anim, Action<string, int, float> rpcRunMotion, Katana playerWeapon)
        {
            this.playerUnit = player;
            kcc = cc;
            animator = anim;
            input = new();
            this.playerWeapon = playerWeapon;

            this.rpcRunMotion = rpcRunMotion;

            foreach (var state in stateArray)
            {
                state.InjectFSM(this);
            }

            SetState<PlayerMovementState>();
        }

        public void RPC_RunMotion(string motionName, int startTick, float transitionTime)
        {
            if (!HasAuthority) return;
            rpcRunMotion?.Invoke(motionName, startTick, transitionTime);
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

        public void UpdateTick(InputData newInput, int tick, float deltaTime)
        {
            input.Update(newInput);
            this.cachedTick = tick;
            this.deltaTime = deltaTime;

            currentState?.OnState();
        }

        public void UpdateRender(int tick, float deltaTime)
        {
            this.cachedTick = tick;
            this.deltaTime = deltaTime;

            currentState?.OnRender();
        }

        public void OnAnimEvent(string param)
        {
            currentState?.OnAnimEvent(param);
        }
    }
}