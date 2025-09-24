using UnityEngine;
using Fusion;

namespace Unit
{

    public class PlayerLandState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Land;

        protected override StatePriorityType Priority => StatePriorityType.Free;

        [SerializeField] private float landingMotionMinDuration;
        [SerializeField] private float landingMotionDuration;
        
        private int minLandingMotionEndTick;
        private int landingMotionEndTick;

        private MoveInfo currentMoveInfo;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = ((StateInfo)info).moveInfo;

        protected override void EnterStateAuthority(int enterTick)
        {
            minLandingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionMinDuration * Runner.TickRate);
            landingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionDuration * Runner.TickRate);
        }

        protected override void EnterStateShared(int enterTick)
        {
            PlayAnim(true ? "_LandingWait" : "_LandingMove", .1f, enterTick);
        }

        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if ((!fsm.input.Current.IsInputEmpty() && Runner.Tick > minLandingMotionEndTick) 
                || Runner.Tick > landingMotionEndTick)
            {
                fsm.SetState<PlayerMovementState>();
            }
        }
    }
}