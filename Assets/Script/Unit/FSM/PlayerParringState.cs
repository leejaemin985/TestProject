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

        protected override void SetInfo(INetworkStruct info)
        {
            HitInfo hitInfo = (HitInfo)info;
            receivedHitInfo = hitInfo;
            Debug.Log($"Test - Update Pos : {hitInfo.attackerPos}");
        }

        protected override void EnterState(bool sync = true)
        {
            base.EnterState(sync);

            parringEndTick = Runner.Tick + Mathf.RoundToInt(parringMotionDuration * Runner.TickRate);
            PlayAnim("_Parring_1", 0, sync);
        }


        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            Debug.Log($"Test - {receivedHitInfo.attackerPos}");
            cc.SetLookRotation(Quaternion.LookRotation(receivedHitInfo.attackerPos));

            if (Runner.Tick >= parringEndTick)
            {
                fsm.SetState<PlayerDefenseState>();
                return;
            }
        }
    }
}