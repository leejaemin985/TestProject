using UnityEngine;
using Fusion;

namespace Unit
{
    public class PlayerParringState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Parring;
        public const float parringMotionDuration = .4f;

        private int parringEndTick;

        private HitInfo receivedHitInfo;
        private float curvSpeed = 10;

        private int parringPushEndTick;

        private const float parringPushTime = .15f;
        private float parringPushSpeed = 200f;
        

        protected override void SetInfo(INetworkStruct info) => receivedHitInfo = (HitInfo)info;

        protected override void EnterState(bool sync = true)
        {
            base.EnterState(sync);

            parringEndTick = Runner.Tick + Mathf.RoundToInt(parringMotionDuration * Runner.TickRate);
            PlayAnim($"_Parring_{Random.Range(1, 5)}", 0.1f, sync);

            parringPushEndTick = Runner.Tick + Mathf.RoundToInt(parringPushTime * Runner.TickRate);
        }


        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if (Runner.Tick >= parringEndTick)
            {
                fsm.SetState<PlayerDefenseState>();
                return;
            }

            if (Runner.Tick < parringPushEndTick)
            {
                Vector3 dir = (receivedHitInfo.attackerPos - player.transform.position).normalized;
                cc.SetLookRotation(Quaternion.Slerp(cc.transform.rotation, Quaternion.LookRotation(dir), curvSpeed * Runner.DeltaTime));

                var targetDir = -dir * receivedHitInfo.weight;
                cc.Move(targetDir * parringPushSpeed * Runner.DeltaTime);
            }
        }

        protected override void OnEnterRender()
        {
            this.weap.SetParringEffectActive(player.transform.position + Vector3.up, Quaternion.identity);
        }
    }
}