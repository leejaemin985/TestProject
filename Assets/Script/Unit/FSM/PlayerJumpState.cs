namespace Unit
{
    using Fusion;
    using Unity.VisualScripting;
    using UnityEngine;

    public class PlayerJumpState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Jump;

        [SerializeField] private float jumpVelocity;

        [Networked] private float currentVelocity { get; set; }

        private Vector3 currentMoveDir;
        private float velocity;

        protected override void SetInfo(INetworkStruct info)
        {
            MoveInfo moveInfo = (MoveInfo)info;
            currentMoveDir = moveInfo.moveDir;
            velocity = moveInfo.velocity;
        }

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

            cc.Move(currentMoveDir * velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            currentVelocity = Mathf.Clamp01(currentVelocity - Runner.DeltaTime);

            anim.SetFloat("_JumpVelocity", currentVelocity);
        }
    }
}