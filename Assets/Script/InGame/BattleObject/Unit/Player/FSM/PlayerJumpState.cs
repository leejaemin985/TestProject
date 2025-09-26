using UnityEngine;
using Fusion;

namespace Unit
{

    public class PlayerJumpState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Jump;

        protected override StatePriorityType Priority => StatePriorityType.Free;

        [SerializeField] private float jumpVelocity;

        [Networked] private float currentVelocity { get; set; }

        private MoveInfo currentMoveInfo;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = ((StateInfo)info).moveInfo;

        #region FSM State
        //EnterState
        protected override void EnterStateAuthority(int enterTick)
        {
            currentVelocity = 0;
            Move(default, jumpVelocity);
        }

        protected override void EnterStateShared(int enterTick)
        {
            PlayAnim("_Jump", .1f, enterTick);
        }

        //OnState
        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if (IsGrounded())
            {
                fsm.SetState<PlayerLandState>();
                return;
            }

            currentVelocity = Mathf.Clamp01(currentVelocity - Runner.DeltaTime);
            Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            modelAnim.SetFloat("_JumpVelocity", currentVelocity);
        }
        #endregion
    }
}