using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using static Unit.PlayerStateBase;

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

        private IState currentState;

        public IState CurrentState
        {
            get { return currentState; }
            private set
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

            CurrentState = stateMap[PlayerStateBase.StateType.Move];

            isInitialized = true;

            StartTestMode();
        }

        public void AddChangeStateListener(Action<PlayerStateBase.StateType> changeStateTypeListener)
        {
            this.changeStateTypeListener += changeStateTypeListener;
        }

        public HitResultType CheckHittable(HitInfo hitInfo)
        {
            bool inDefense = 
                CurrentState.GetStateType() == PlayerStateBase.StateType.Defense || 
                CurrentState.GetStateType() == PlayerStateBase.StateType.Parring;

            float dirDot = Vector3.Dot((hitInfo.attackerPos - player.transform.position).normalized, player.transform.forward);
            bool attackIsForward = dirDot > 0;

            if (inDefense && attackIsForward) return HitResultType.Parry;


            bool isDied = player.GetHp() - hitInfo.damaged <= 0;
            if (isDied)
                return HitResultType.Died;
            else
                return HitResultType.Hit;
        }

        public void OnIdleState()
        {
            SetState<PlayerMovementState>(TransitionType.System, false);
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
            bool systemTransition = transitionType == TransitionType.System;
            bool priorityRequest = CurrentState.priority <= priorityType;

            return systemTransition || priorityRequest;
        }

        public bool CanSetState(TransitionType transitionType, IState state)
        {
            if (CurrentState == null) return true;

            bool systemTransition = transitionType == TransitionType.System;
            bool priorityRequest = CurrentState.priority <= state.priority;

            return systemTransition || priorityRequest;
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
                if (sync) RPC_SyncState(transitionType, CurrentState.GetStateType());
            }

            changeStateTypeListener?.Invoke(CurrentState.GetStateType());
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
                if (sync) RPC_SyncState(transitionType, CurrentState.GetStateType());
            }

            changeStateTypeListener?.Invoke(CurrentState.GetStateType());
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SyncState(TransitionType transitionType, PlayerStateBase.StateType stateType)
        {
            if (HasStateAuthority || CanSetState(transitionType, stateMap[stateType]) == false) return;

            CurrentState = stateMap[stateType];
        }

        public override void Render()
        {
            if (isInitialized == false) return;

            CurrentState?.OnRender();

            if (Runner.IsSharedModeMasterClient) CurrentState?.OnMasterTick();
        }

        #region Test
        private bool test_attackInput = false;
        private bool test_hasRoar = true;

        private const float test_attackTerm = 3f;
        private const float test_roarTerm = 10;

        private void StartTestMode()
        {
            StartCoroutine(AttackTest());
            StartCoroutine(RoarTest());
        }

        private IEnumerator AttackTest()
        {
            while (true)
            {
                yield return new WaitForSeconds(test_attackTerm);
                test_attackInput = !test_attackInput;
            }
        }

        private IEnumerator RoarTest()
        {
            while (true)
            {
                if (test_hasRoar == false)
                {
                    yield return new WaitForSeconds(test_roarTerm);
                    test_hasRoar = true;
                }

                yield return null;
            }
        }
        #endregion

        public override void FixedUpdateNetwork()
        {
            if (isInitialized == false || player.canControll == false) return;

            if (GetInput<InputData>(out var newInput) == false) return;

            if (false) // testMode
            {
                if (CurrentState.GetStateType() == StateType.Hit && test_hasRoar)
                {
                    newInput.skill = true;
                    test_hasRoar = false;
                }
                else
                {
                    newInput.attack = test_attackInput;
                }
            }
            
            input.Update(newInput);

            CurrentState?.OnState();
        }

        public void AnimEvent(string param)
        {
            CurrentState?.OnAnimEvent(param);
        }
    }
}