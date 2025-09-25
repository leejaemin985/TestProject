using UnityEngine;
using Fusion;

namespace Unit
{
    public class PlayerParringState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Parring;

        protected override StatePriorityType Priority => StatePriorityType.Event;

        private const float parringMotionDuration = .4f;

        private int parringEndTick;

        private HitInfo receivedHitInfo;
        private float curvSpeed = 10;

        private int parringPushEndTick;

        private const float parringPushTime = .15f;
        private float parringPushSpeed = 200f;


        protected override void SetInfo(INetworkStruct info) => receivedHitInfo = ((StateInfo)info).hitInfo;

        protected override void EnterStateShared(int enterTick)
        {
            parringEndTick = Runner.Tick + Mathf.RoundToInt(parringMotionDuration * Runner.TickRate);
            parringPushEndTick = Runner.Tick + Mathf.RoundToInt(parringPushTime * Runner.TickRate);

            PlayAnim($"_Parring_{Random.Range(1, 5)}", 0.1f, enterTick);

            this.weap.SetParringEffectActive(player.transform.position + Vector3.up, Quaternion.identity);
        }


        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            if (Runner.Tick >= parringEndTick)
            {
                fsm.SetState<PlayerDefenseState>(default, TransitionType.System);
                return;
            }

            if (Runner.Tick < parringPushEndTick)
            {
                Vector3 dir = (receivedHitInfo.attackerPos - player.transform.position).normalized;
                SetLookRotation(Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(dir), curvSpeed * Runner.DeltaTime));

                var targetDir = -dir * receivedHitInfo.weight;
                Move(targetDir * parringPushSpeed * Runner.DeltaTime);
            }
        }
    }
}