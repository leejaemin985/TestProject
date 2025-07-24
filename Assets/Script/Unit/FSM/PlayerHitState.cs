using UnityEngine;
using System.Runtime.CompilerServices;
using Fusion;

namespace Unit
{
    public class PlayerHitState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Hit;

        public const float hitMotionDuration = 1f;
        private int hitEndTick;

        private HitInfo currentHitInfo;

        protected override void SetInfo(INetworkStruct info)
        {
            HitInfo hitInfo = (HitInfo)info;
            if (hitInfo.damaged == 0) return;
            Debug.Log($"Test - HitInfo dmg: {hitInfo.damaged} // weight: {hitInfo.weight} // type: {hitInfo.attackType}");
            currentHitInfo = hitInfo;
        }

        protected override void EnterState(bool sync = true)
        {
            base.EnterState();

            hitEndTick = Runner.Tick + Mathf.RoundToInt(hitMotionDuration * Runner.TickRate);

            PlayAnim("_HitF", 0, sync);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (Runner.Tick >= hitEndTick)
            {
                fsm.SetState<PlayerMovementState>();
            }
        }
    }
}