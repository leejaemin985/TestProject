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

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = (MoveInfo)info;

        protected override void EnterState(PlayerFSM.TransitionType transitionType, bool sync = true)
        {
            minLandingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionMinDuration * Runner.TickRate);
            landingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionDuration * Runner.TickRate);

            PlayAnim(transitionType, Priority, true ? "_LandingWait" : "_LandingMove", .1f, true);
        }

        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if ((!fsm.input.Current.IsInputEmpty() && Runner.Tick > minLandingMotionEndTick) 
                || Runner.Tick > landingMotionEndTick)
            {
                fsm.SetState<PlayerMovementState>(PlayerFSM.TransitionType.Request);
            }
        }
    }
}