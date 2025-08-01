namespace Unit
{
    using Fusion;
    using UnityEngine;

    public class PlayerLandState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Land;

        [SerializeField] private float landingMotionMinDuration;
        [SerializeField] private float landingMotionDuration;
        
        private int minLandingMotionEndTick;
        private int landingMotionEndTick;

        private MoveInfo currentMoveInfo;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = (MoveInfo)info;

        protected override void EnterState(bool sync = true)
        {
            minLandingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionMinDuration * Runner.TickRate);
            landingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionDuration * Runner.TickRate);

            //bool running = currentMoveInfo.velocity > 110;
            PlayAnim(true ? "_LandingWait" : "_LandingMove", .1f, true);
        }

        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if ((!fsm.input.Current.IsInputEmpty() && Runner.Tick > minLandingMotionEndTick) 
                || Runner.Tick > landingMotionEndTick)
            {
                fsm.SetState<PlayerMovementState>();
            }

            //cc.Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }
    }
}