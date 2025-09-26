using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using static Unit.PlayerStateBase;
using System.Linq;

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
                currentState = value;
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
            bool localSuperArmor = player.UnitStat.Object.StateAuthority != Runner.LocalPlayer && CurrentState.HasSuperArmor;
            bool statSuperArmor = player.UnitStat.superArmor;

            if (localSuperArmor == false && statSuperArmor == false)
                SetState<PlayerHitState>(new StateInfo() { hitInfo = hitInfo }, TransitionType.System, false);
            else
            {
                systemSeq++;
            }
        }

        public void OnParringState(HitInfo hitInfo)
        {
            SetState<PlayerParringState>(new StateInfo() { hitInfo = hitInfo }, TransitionType.System, false);
        }

        public void OnDiedState(HitInfo hitInfo)
        {
            SetState<PlayerDiedState>(default, TransitionType.System, false);
        }


        public bool CanSetState(StateTransitionData transitionData)
        {
            if (transitionData.transitionType == TransitionType.System) return systemSeq < transitionData.systemSeq;
            
            IState state = stateMap[transitionData.stateType];
            return CurrentState.priority <= state.priority;
        }

        private bool TryGetIState<T>(out IState? ret) where T : IState
        {
            ret = stateArray.FirstOrDefault(x => x is T State);
            return ret != null;
        }

        public void SetState<TState>(StateInfo stateInfo = default, TransitionType transitionType = TransitionType.Request, bool requestSync = true)
            where TState : class, IState
        {
            if (TryGetIState<TState>(out var state) == false) return;

            StateTransitionData transitionData = new()
            {
                transitionType = transitionType,
                systemSeq = systemSeq + 1,
                stateType = state.GetStateType(),
                stateInfo = stateInfo,
                tick = Runner.Tick
            };

            if (requestSync)
                RPC_RequestSetState(transitionData, stateInfo);
            else
                SetState(transitionData, stateInfo);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_RequestSetState(StateTransitionData transitionData, StateInfo stateInfo)
        {
            SetState(transitionData, stateInfo);
        }

        private void SetState(StateTransitionData transitionData, StateInfo stateInfo)
        {
            if (CanSetState(transitionData) == false) return;

            if (transitionData.transitionType == TransitionType.System)
                systemSeq = transitionData.systemSeq;

            CurrentState?.ExitState();
            CurrentState = stateMap[transitionData.stateType];
            CurrentState?.SetInfo(stateInfo);
            CurrentState?.EnterState(transitionData.tick);

            changeStateTypeListener?.Invoke(transitionData.stateType);
        }

        PlayerStateBase.StateType? lastState;

        public override void Render()
        {
            if (HasStateAuthority == false && CurrentState != null) 
            {
                if (lastState == null || lastState != CurrentState.GetStateType())
                {
                    lastState = CurrentState.GetStateType();
                    Debug.Log($"Test - State: {lastState}");
                }
            }

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