using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{
    public class PlayerRoarState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Roar;

        protected override StatePriorityType Priority => StatePriorityType.Override;

        private const float roarMotionDuration = .4f;

        private int roarEndTick;

        protected override void EnterState(PlayerFSM.TransitionType transitionType, bool sync = true)
        {
            roarEndTick = Runner.Tick + Mathf.RoundToInt(roarMotionDuration * Runner.TickRate);
            PlayAnim(transitionType, Priority, "Roar", .1f, true);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (Runner.Tick > roarEndTick)
            {
                fsm.SetState<PlayerMovementState>(PlayerFSM.TransitionType.System);
            }
        }
    }
}
