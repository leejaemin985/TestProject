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

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = (MoveInfo)info;

        protected override void EnterState(PlayerFSM.TransitionType transitionType, bool sync = true)
        {
            currentVelocity = 0;
            PlayAnim(transitionType, Priority, "_Jump", .1f, true);

            cc.Move(default, jumpVelocity);
        }

        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if (cc.IsGrounded)
            {
                fsm.SetState<PlayerLandState>(PlayerFSM.TransitionType.Request);
                return;
            }

            cc.Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            currentVelocity = Mathf.Clamp01(currentVelocity - Runner.DeltaTime);

            modelAnim.SetFloat("_JumpVelocity", currentVelocity);
        }
    }
}