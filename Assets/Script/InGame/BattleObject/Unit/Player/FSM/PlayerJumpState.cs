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

        protected override void EnterState(int enterTick)
        {
            PlayAnim("_Jump", .1f, enterTick);

            currentVelocity = 0;
            Move(default, jumpVelocity);
        }

        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if (IsGrounded())
            {
                fsm.SetState<PlayerLandState>();
                return;
            }

            Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            currentVelocity = Mathf.Clamp01(currentVelocity - Runner.DeltaTime);

            modelAnim.SetFloat("_JumpVelocity", currentVelocity);
        }
    }
}