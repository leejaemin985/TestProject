using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;

namespace Unit
{
    public class PlayerFSM : NetworkBehaviour, IMachineState
    {
        private bool isInitialized = false;

        private Player player;

        [SerializeField] private PlayerStateBase[] stateArray = default;

        private Dictionary<PlayerStateBase.StateType, PlayerStateBase> stateMap;


        public InputInterpreter input;


        public enum HitResultType
        {
            Hit,
            Parry,
            Died
        }

        [Networked] public PlayerStateBase.StateType currentStateType { get; set; }

        private IState currentState;

        private IState CurrentState
        {
            get { return currentState; }
            set
            {
                currentState?.OnExitRender();
                currentState = value;
                currentState?.OnEnterRender();
            }
        }

        public void Initialized(Player player, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, IWeapon playerWeapon)
        {
            this.player = player;
            input = new();

            stateMap = new();
            foreach (var state in stateArray)
            {
                state.Initialize(player, this, cc, modelAnim, latencyInterpolationAnim, playerWeapon);
                stateMap[state.GetStateType()] = state;
            }

            CurrentState = stateMap[currentStateType];

            isInitialized = true;

            StartCoroutine(TestCoroutine());
        }

        public HitResultType CheckHittable(HitInfo hitInfo)
        {
            bool inDefense = 
                currentStateType == PlayerStateBase.StateType.Defense || 
                currentStateType == PlayerStateBase.StateType.Parring;

            float dirDot = Vector3.Dot((hitInfo.attackerPos - player.transform.position).normalized, player.transform.forward);
            bool attackIsForward = dirDot > 0;

            if (inDefense && attackIsForward) return HitResultType.Parry;


            bool isDied = player.GetHp() - hitInfo.damaged <= 0;

            if (isDied)
                return HitResultType.Died;
            else
                return HitResultType.Hit;
        }

        public void OnHitState(HitInfo hitInfo)
        {
            SetState<PlayerHitState, HitInfo>(hitInfo, false);

            OnStateLock(PlayerHitState.hitMotionDuration);
        }

        public void OnParringState(HitInfo hitInfo)
        {
            SetState<PlayerParringState, HitInfo>(hitInfo, false);
            OnStateLock(PlayerParringState.parringMotionDuration);
        }

        public void OnDiedState(HitInfo hitInfo)
        {
            SetState<PlayerDiedState>(false);
            OnStateLock(5);
        }

        private void OnStateLock(float sec)
        {
            if (!Runner.IsSharedModeMasterClient || HasStateAuthority) return;

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
                    CurrentState?.ExitState();
                    CurrentState = state;
                    CurrentState?.EnterState(sync);
                    break;
                }
            }

            if (HasStateAuthority)
            {
                currentStateType = CurrentState.GetStateType();
                if (sync) RPC_SyncState();
            }
        }

        public void SetState<TState, TInfo>(TInfo info, bool sync = true)
            where TState : PlayerStateBase, IState
            where TInfo:struct, INetworkStruct
        {
            for (int index = 0, max = stateArray.Length; index < max; ++index)
            {
                if (stateArray[index] is TState state)
                {
                    CurrentState?.ExitState();
                    CurrentState = state;
                    CurrentState.SetInfo(info);
                    CurrentState?.EnterState(sync);
                    break;
                }
            }

            if (HasStateAuthority)
            {
                currentStateType = CurrentState.GetStateType();
                if (sync) RPC_SyncState();
            }
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SyncState()
        {
            if (isStateLockActive || HasStateAuthority) return;
            CurrentState = stateMap[currentStateType];
        }

        public override void Render()
        {
            if (isInitialized == false) return;

            CurrentState?.OnRender();

            if (Runner.IsSharedModeMasterClient) CurrentState?.OnMasterTick();
        }

        #region Test
        private bool isTest = false;
        private IEnumerator TestCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(3);
                isTest = !isTest;
            }
        }
        #endregion

        public override void FixedUpdateNetwork()
        {
            if (isInitialized == false || player.canControll == false) return;

            if (GetInput<InputData>(out var newInput) == false) return;

            //newInput.attack = isTest;
            input.Update(newInput);

            CurrentState?.OnState();
        }

        public void AnimEvent(string param)
        {
            CurrentState?.OnAnimEvent(param);
        }
    }
}