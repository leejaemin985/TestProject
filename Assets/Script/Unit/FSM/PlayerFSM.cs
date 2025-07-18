using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unit
{
    public class PlayerFSM : MonoBehaviour, IMachineState
    {
        public const string RESOURCES_PATH = "Prefab/PlayerFSM";

        [SerializeField] private PlayerStateBase[] stateArray = default;

        private Dictionary<PlayerStateBase.StateType, PlayerStateBase> stateMap;

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
        private Action<PlayerStateBase.StateType> syncState;

        public void Initialized(Player player, SimpleKCC cc, Animator anim, Action<string, int, float> rpcRunMotion, Action<PlayerStateBase.StateType> syncState, Katana playerWeapon)
        {
            this.playerUnit = player;
            kcc = cc;
            animator = anim;
            input = new();
            this.playerWeapon = playerWeapon;

            this.rpcRunMotion = rpcRunMotion;
            this.syncState = syncState;
            stateMap = new();

            foreach (var state in stateArray)
            {
                state.InjectFSM(this);
                stateMap[state.stateType] = state;
            }

            SetState<PlayerMovementState>();
        }

        public void RPC_SyncState(PlayerStateBase.StateType stateType) => syncState?.Invoke(stateType);

        public void RPC_RunMotion(string motionName, int startTick, float transitionTime) => rpcRunMotion?.Invoke(motionName, startTick, transitionTime);


        public void AdjustSetState(PlayerStateBase.StateType stateType)
        {
            if (HasAuthority) return;

            if (stateMap.TryGetValue(stateType, out var state))
            {
                currentState = state;
            }
        }

        public void SetState<T>() where T : class, IState
        {
            if (!HasAuthority) return;

            for (int index = 0, max = stateArray.Length; index < max; ++index)
            {
                if (stateArray[index] is T state)
                {
                    currentState?.ExitState();
                    currentState = state;
                    currentState?.EnterState();
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

        public void UpdateRender()
        {
            currentState?.OnRender();
        }

        public void OnAnimEvent(string param)
        {
            currentState?.OnAnimEvent(param);
        }
    }
}