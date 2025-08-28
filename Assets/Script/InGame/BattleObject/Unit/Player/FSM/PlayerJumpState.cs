using UnityEngine;
using Fusion;

namespace Unit
{

    public class PlayerJumpState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Jump;

        [SerializeField] private float jumpVelocity;

        [Networked] private float currentVelocity { get; set; }

        private MoveInfo currentMoveInfo;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = (MoveInfo)info;

        protected override void EnterState(bool sync = true)
        {
            currentVelocity = 0;
            PlayAnim("_Jump", .1f, true);

            cc.Move(default, jumpVelocity);
        }

        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if (cc.IsGrounded)
            {
                fsm.SetState<PlayerLandState>();
                return;
            }

            cc.Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            currentVelocity = Mathf.Clamp01(currentVelocity - Runner.DeltaTime);

            anim.SetFloat("_JumpVelocity", currentVelocity);
        }
    }
}