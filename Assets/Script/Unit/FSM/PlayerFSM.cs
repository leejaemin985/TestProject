using System.Collections;
using System.Collections.Generic;

using Fusion;
using Fusion.Addons.SimpleKCC;

using UnityEngine;

namespace Unit
{
    public class PlayerFSM : NetworkBehaviour, IMachineState
    {
        [SerializeField] private PlayerStateBase[] stateArray = default;

        private bool isInitialized = false;

        private Dictionary<PlayerStateBase.StateType, PlayerStateBase> stateMap;

        public InputInterpreter input;

        [Networked] public PlayerStateBase.StateType currentStateType { get; set; }

        private IState currentState;

        public override void Spawned()
        {
            if (initSequencerHandle != null) StopCoroutine(initSequencerHandle);
            StartCoroutine(initSequencerHandle = InitSequencer());
        }

        private IEnumerator initSequencerHandle = null;

        private IEnumerator InitSequencer()
        {
            yield return new WaitUntil(() => isInitialized);
            currentState = stateMap[currentStateType];
        }

        public void Initialized(Player player, SimpleKCC cc, Animator anim, Katana playerWeapon)
        {
            input = new();

            stateMap = new();
            foreach (var state in stateArray)
            {
                state.Initialize(player, this, cc, anim, playerWeapon);
                stateMap[state.GetStateType()] = state;
            }

            currentState = stateMap[currentStateType];

            isInitialized = true;

            StartCoroutine(Test());
        }

        public void OnHitState(HitInfo hitInfo)
        {
            if (currentStateType == PlayerStateBase.StateType.Defense) return;
            SetState<PlayerHitState>(false);

            OnStateLock(PlayerHitState.hitMotionDuration);
        }

        private void OnStateLock(float sec)
        {
            if (!Runner.IsSharedModeMasterClient) return;

            isStateLockActive = true;
            if (stateLockCoroutineHandle != null) StartCoroutine(stateLockCoroutineHandle);
            StartCoroutine(stateLockCoroutineHandle = StateLockCoroutine(sec));
        }

        public bool isStateLockActive { get; private set; }
        private IEnumerator stateLockCoroutineHandle = null;
        private IEnumerator StateLockCoroutine(float sec)
        {
            yield return new WaitForSeconds(sec);
            isStateLockActive = false;
        }

        public void SetState<T>(bool sync = true) where T : class, IState
        {
            for (int index = 0, max = stateArray.Length; index < max; ++index)
            {
                if (stateArray[index] is T state)
                {
                    currentState?.ExitState();
                    currentState = state;
                    currentState?.EnterState(sync);
                    break;
                }
            }

            if (HasStateAuthority) currentStateType = currentState.GetStateType();
            if (sync) RPC_SyncState();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SyncState()
        {
            if (isStateLockActive) return;
            currentState = stateMap[currentStateType];
        }

        public override void Render()
        {
            if (isInitialized == false) return;

            currentState?.OnRender();
        }

        #region TestCode
        private bool attack = true;

        private IEnumerator Test()
        {
            while (true)
            {
                yield return new WaitForSeconds(3f);
                attack = !attack;
            }
        }
        #endregion

        public override void FixedUpdateNetwork()
        {
            if (isInitialized == false) return;

            if (GetInput<InputData>(out var newInput) == false) return;

            //newInput.attack = attack;
            
            input.Update(newInput);

            currentState?.OnState();
        }

        public void AnimEvent(string param)
        {
            currentState?.OnAnimEvent(param);
        }
    }
}