using UnityEngine;
using System.Runtime.CompilerServices;

namespace Unit
{
    public class PlayerHitState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Hit;

        [SerializeField] private float hitMotionDuration;
        private int hitEndTick;

        protected override void EnterState()
        {
            base.EnterState();

            hitEndTick = Runner.Tick + Mathf.RoundToInt(hitMotionDuration * Runner.TickRate);

            PlayAnim("_HitF", 0);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (Runner.Tick >= hitEndTick)
            {
                fsm.SetState<PlayerMovementState>();
            }
        }
    }
}