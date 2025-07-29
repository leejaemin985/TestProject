using UnityEngine;

namespace Unit
{
    public class PlayerParringState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Parring;
        public const float parringMotionDuration = .5f;

        private int parringEndTick;

        protected override void EnterState(bool sync = true)
        {
            base.EnterState(sync);

            parringEndTick = Runner.Tick + Mathf.RoundToInt(parringMotionDuration * Runner.TickRate);
            PlayAnim("_Parring_1", 0, sync);
        }


        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if (Runner.Tick >= parringEndTick)
            {
                fsm.SetState<PlayerDefenseState>();
                return;
            }
        }
    }
}