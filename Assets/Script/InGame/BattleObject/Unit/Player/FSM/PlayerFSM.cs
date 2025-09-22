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

        public int systemSeq { get; private set; }

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

            StartTest();
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

        private bool IsValidSystemTransition(TransitionType transitionType, int seq)
        {
            if (transitionType != TransitionType.System) return false;
            if (Runner.IsSharedModeMasterClient == false)
                return true;

            return systemSeq < seq;
        }

        public bool CanSetState(TransitionType transitionType, IState state, int seq)
        {
            if (CurrentState == null) return true;
            return CanSetState(transitionType, state.priority, seq);
        }

        public bool CanSetState(TransitionType transitionType, PlayerStateBase.StatePriorityType priorityType, int seq)
        {
            bool systemTransition = IsValidSystemTransition(transitionType, seq);
            bool priorityRequest = CurrentState.priority <= priorityType;

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
            int requestSeq = systemSeq + (Runner.IsSharedModeMasterClient ? 1 : 0);
            if (TryGetIState<TState>(out var state) == false) return;
            if (CanSetState(transitionType, state, requestSeq) == false) return;

            systemSeq = requestSeq;

            CurrentState?.ExitState();
            CurrentState = state;
            CurrentState?.EnterState(transitionType, sync);

            if (HasStateAuthority)
            {
                if (sync) RPC_SyncState(transitionType, CurrentState.GetStateType(), systemSeq);
            }

            changeStateTypeListener?.Invoke(CurrentState.GetStateType());
        }

        public void SetState<TState, TInfo>(TransitionType transitionType, TInfo info, bool sync = true)
            where TState : PlayerStateBase, IState
            where TInfo:struct, INetworkStruct
        {
            int requestSeq = systemSeq + 1;
            if (TryGetIState<TState>(out var state) == false) return;
            if (CanSetState(transitionType, state, requestSeq) == false) return;

            systemSeq = requestSeq;

            if (HasStateAuthority == false && transitionType == TransitionType.System)
            {
                if (state.GetStateType() == StateType.Hit)
                {
                    Debug.Log($"Test - hit (tick: {Runner.Tick} // seq: {systemSeq})");
                }
            }


            CurrentState?.ExitState();
            CurrentState = state;
            CurrentState.SetInfo(info);
            CurrentState?.EnterState(transitionType, sync);

            if (HasStateAuthority)
            {
                if (sync) RPC_SyncState(transitionType, CurrentState.GetStateType(), systemSeq);
            }

            changeStateTypeListener?.Invoke(CurrentState.GetStateType());
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SyncState(TransitionType transitionType, PlayerStateBase.StateType stateType, int seq)
        {
            if (HasStateAuthority == false && transitionType == TransitionType.System)
            {
                if (stateType == StateType.Move)
                {
                    Debug.Log($"Test - move (tick: {Runner.Tick} // seq: {seq})");
                }

            }



            if (HasStateAuthority || CanSetState(transitionType, stateMap[stateType], seq) == false) return;

            CurrentState = stateMap[stateType];
        }

        public override void Render()
        {
            if (isInitialized == false) return;

            CurrentState?.OnRender();

            if (Runner.IsSharedModeMasterClient) CurrentState?.OnMasterTick();
        }

        #region Test

        private bool isTest = false;
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) isTest = !isTest;
        }

        private bool test_attackInput = false;
        private bool test_hasRoar = true;

        private void StartTest()
        {
            StartCoroutine(Test_AttackRoutine());
            StartCoroutine(Test_RoarSetting());
        }

        private IEnumerator Test_AttackRoutine()
        {
            WaitForSeconds term = new WaitForSeconds(3);
            while (true)
            {
                yield return term;
                test_attackInput = !test_attackInput;
            }
        }

        private IEnumerator Test_RoarSetting()
        {
            WaitForSeconds term = new WaitForSeconds(10);
            while (true)
            {
                if (test_hasRoar == false)
                {
                    yield return term;
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

            #region Test
            if (isTest)
            {
                if (CurrentState.GetStateType() == StateType.Hit && test_hasRoar && false) 
                {
                    newInput.skill = true;
                    test_hasRoar = false;
                }
                else
                {
                    newInput.attack = test_attackInput;
                }
            }
            #endregion
            input.Update(newInput);

            CurrentState?.OnState();
        }

        public void AnimEvent(string param)
        {
            CurrentState?.OnAnimEvent(param);
        }
    }
}