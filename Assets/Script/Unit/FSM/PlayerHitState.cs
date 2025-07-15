using System;
using UnityEngine;

namespace Unit
{
    public class PlayerHitState : PlayerStateBase
    {
        [SerializeField]
        private MotionInfo[] hitMotionInfos;

        private int hitEndTick;

        private Vector3 currentHitMove;
        private float hitMoveSpeed;

        protected override void EnterState()
        {
            var currentMotion = hitMotionInfos[0];

            currentHitMove = Vector3.zero;
            float tickRate = 1 / fsm.deltaTime;
            hitEndTick = fsm.cachedTick + Mathf.RoundToInt(currentMotion.motionDuration * tickRate);

            fsm.RPC_RunMotion(currentMotion.motionName, fsm.cachedTick, 0);
        }

        protected override void OnState()
        {
            if (!fsm.HasAuthority) return;
            if (hitEndTick <= fsm.cachedTick)
            {
                fsm.SetState<PlayerMovementState>();
            }

            fsm.cc.Move(currentHitMove * hitMoveSpeed * fsm.deltaTime);
        }

        protected override void ExitState()
        {
            currentHitMove = Vector3.zero;
        }
    }
}