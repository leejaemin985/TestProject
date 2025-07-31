namespace Unit
{
    using UnityEngine;

    public class PlayerLandState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Land;

        [SerializeField] private float landingMotionMinDuration;
        [SerializeField] private float landingMotionDuration;
        
        private int minLandingMotionEndTick;
        private int landingMotionEndTick;

        protected override void EnterState(bool sync = true)
        {
            minLandingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionMinDuration * Runner.TickRate);
            landingMotionEndTick = Runner.Tick + Mathf.RoundToInt(landingMotionDuration * Runner.TickRate);

            PlayAnim("_Landing", .1f, true);
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