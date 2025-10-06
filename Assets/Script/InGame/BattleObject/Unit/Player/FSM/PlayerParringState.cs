using UnityEngine;
using Fusion;

namespace Unit
{
    public class PlayerParringState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Parring;

        protected override StatePriorityType Priority => StatePriorityType.Event;

        private const float MOTION_DURATION = .4f;
        private const float PARRING_PUSH_TIME = .15f;

        private HitInfo receivedHitInfo;
        private const float CURV_SPEED = 10;

        private int parringEndTick;
        private int parringPushEndTick;

        private float parringPushSpeed = 200f;

        protected override void SetInfo(INetworkStruct info) => receivedHitInfo = ((StateInfo)info).hitInfo;

        #region FSM State
        //EnterState
        protected override void EnterStateShared(int enterTick)
        {
            parringEndTick = Runner.Tick + Mathf.RoundToInt(MOTION_DURATION * Runner.TickRate);
            parringPushEndTick = Runner.Tick + Mathf.RoundToInt(PARRING_PUSH_TIME * Runner.TickRate);

            PlayAnim($"_Parring_{Random.Range(1, 5)}", 0.1f, enterTick);

            weap.SetParringEffectActive(player.transform.position + Vector3.up, Quaternion.identity);
            weap.PlayWeapSE(InGame.Weapon.WeapSoundObject.WeapSoundType.Collision);
        }

        //OnState
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
                SetLookRotation(Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(dir), CURV_SPEED * Runner.DeltaTime));

                var targetDir = -dir * receivedHitInfo.weight;
                Move(targetDir * parringPushSpeed * Runner.DeltaTime);
            }
        }
        #endregion
    }
}