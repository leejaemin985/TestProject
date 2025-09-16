using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Unit
{
    public class PlayerFSM : NetworkBehaviour, IMachineState
    {
        private bool isInitialized = false;

        private Player player;

        [SerializeField] private PlayerStateBase[] stateArray = default;

        private Dictionary<PlayerStateBase.StateType, PlayerStateBase> stateMap;
        private Action<PlayerStateBase.StateType> changeStateTypeListener;

        public InputInterpreter input;


        public enum TransitionType
        {
            Request,
            System
        }


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

        public void Initialized(
            Player player,
            SimpleKCC cc,
            Animator modelAnim,
            Animator latencyInterpolationAnim,
            IWeapon playerWeapon)
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

        public void AddChangeStateListener(Action<PlayerStateBase.StateType> changeStateTypeListener)
        {
            this.changeStateTypeListener += changeStateTypeListener;
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
            SetState<PlayerHitState, HitInfo>(TransitionType.System, hitInfo, false);
        }

        public void OnParringState(HitInfo hitInfo)
        {
            SetState<PlayerParringState, HitInfo>(TransitionType.System, hitInfo, false);
        }

        public void OnDiedState(HitInfo hitInfo)
        {
            SetState<PlayerDiedState>(TransitionType.System, false);
        }


        public bool CanSetState(TransitionType transitionType, PlayerStateBase.StatePriorityType priorityType)
        {
            return transitionType == TransitionType.System || CurrentState.priority <= priorityType;
        }

        public bool CanSetState(TransitionType transitionType, IState state)
        {
            if (CurrentState == null) return true;
            return transitionType == TransitionType.System || CurrentState.priority <= state.priority;
        }

        private bool TryGetIState<T>(out IState? ret) where T : IState
        {
            ret = null;

            for (int index = 0, max = stateArray.Length; index < max; ++index)
            {
                if (stateArray[index] is T state)
                {
                    ret = state;
                    return true;
                }
            }
            return false;
        }

        public void SetState<TState>(TransitionType transitionType, bool sync = true)
            where TState : class, IState 
        {
            if (TryGetIState<TState>(out var state) == false) return;
            if (CanSetState(transitionType, state) == false) return;

            CurrentState?.ExitState();
            CurrentState = state;
            CurrentState?.EnterState(transitionType, sync);

            if (HasStateAuthority)
            {
                currentStateType = CurrentState.GetStateType();
                if (sync) RPC_SyncState(transitionType);
            }

            changeStateTypeListener?.Invoke(currentStateType);
        }

        public void SetState<TState, TInfo>(TransitionType transitionType, TInfo info, bool sync = true)
            where TState : PlayerStateBase, IState
            where TInfo:struct, INetworkStruct
        {
            if (TryGetIState<TState>(out var state) == false) return;
            if (CanSetState(transitionType, state) == false) return;

            CurrentState?.ExitState();
            CurrentState = state;
            CurrentState.SetInfo(info);
            CurrentState?.EnterState(transitionType, sync);

            if (HasStateAuthority)
            {
                currentStateType = CurrentState.GetStateType();
                if (sync) RPC_SyncState(transitionType);
            }

            changeStateTypeListener?.Invoke(currentStateType);
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SyncState(TransitionType transitionType)
        {
            if (HasStateAuthority || CanSetState(transitionType, stateMap[currentStateType]) == false) return;

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