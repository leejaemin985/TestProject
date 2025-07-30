namespace Unit
{
    using UnityEngine;

    public class PlayerJumpState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Jump;

        [SerializeField] private float jumpVelocity;

        private float currentVelocity;

        protected override void EnterState(bool sync = true)
        {
            cc.Move(Vector3.zero, 10);
        }

        protected override void OnState()
        {
            cc.Move(Vector3.zero);
        }
    }
}