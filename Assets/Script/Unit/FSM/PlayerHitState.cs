using UnityEngine;
using System.Runtime.CompilerServices;

namespace Unit
{
    public class PlayerHitState : PlayerStateBase
    {
        public override StateType stateType => StateType.Hit;

        [SerializeField] private float hitMotionDuration;
        private int hitEndTick;

        protected override void EnterState()
        {
            base.EnterState();

            float tickRate = 1 / fsm.deltaTime;
            hitEndTick = fsm.cachedTick + Mathf.RoundToInt(hitMotionDuration * tickRate);

            fsm.RPC_RunMotion("_HitF", fsm.cachedTick, 0);
        }

        protected override void OnState()
        {
            if (!fsm.HasAuthority) return;
            if (fsm.cachedTick >= hitEndTick)
            {
                fsm.SetState<PlayerMovementState>();
            }

        }
    }
}