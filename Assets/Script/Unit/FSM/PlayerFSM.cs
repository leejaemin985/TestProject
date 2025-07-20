using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

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

        public void Initialized(Player player, SimpleKCC cc, Animator anim, Katana playerWeapon)
        {
            input = new();

            stateMap = new();
            foreach (var state in stateArray)
            {
                state.Initialize(player, this, cc, anim, playerWeapon);
                stateMap[state.GetStateType()] = state;
            }

            SetState<PlayerMovementState>();

            isInitialized = true;

            StartCoroutine(Test());
        }

        public void SetState(PlayerStateBase.StateType stateType)
        {
            if (stateType == PlayerStateBase.StateType.Hit)
            {
                SetState<PlayerHitState>();
            }
        }

        public void SetState<T>() where T : class, IState
        {
            if (!HasStateAuthority) return;

            for (int index = 0, max = stateArray.Length; index < max; ++index)
            {
                if (stateArray[index] is T state)
                {
                    currentState?.ExitState();
                    currentState = state;
                    currentState?.EnterState();
                    break;
                }
            }
            currentStateType = currentState.GetStateType();
        }

        public override void Render()
        {
            if (isInitialized == false) return;

            if (!HasStateAuthority)
                currentState = stateMap[currentStateType];

            currentState?.OnRender();
        }

        private bool attack = true;

        private IEnumerator Test()
        {
            while (true)
            {
                yield return new WaitForSeconds(3f);
                attack = !attack;
            }
        }


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