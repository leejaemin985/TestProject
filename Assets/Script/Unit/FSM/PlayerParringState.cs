using Fusion;
using UnityEngine;

namespace Unit
{
    public class PlayerParringState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Parring;
        public const float parringMotionDuration = .5f;

        private int parringEndTick;

        private HitInfo receivedHitInfo;
        private float curvSpeed = 10;

        protected override void SetInfo(INetworkStruct info) => receivedHitInfo = (HitInfo)info;

        protected override void EnterState(bool sync = true)
        {
            base.EnterState(sync);

            parringEndTick = Runner.Tick + Mathf.RoundToInt(parringMotionDuration * Runner.TickRate);
            PlayAnim("_Parring_1", 0, sync);
        }


        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            Vector3 dir = (receivedHitInfo.attackerPos - player.transform.position).normalized;
            cc.SetLookRotation(Quaternion.Slerp(cc.transform.rotation, Quaternion.LookRotation(dir), curvSpeed * Runner.DeltaTime));


            if (Runner.Tick >= parringEndTick)
            {
                fsm.SetState<PlayerDefenseState>();
                return;
            }
        }
    }
}