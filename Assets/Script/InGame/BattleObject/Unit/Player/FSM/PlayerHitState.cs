using System;
using System.Linq;

using UnityEngine;

using Fusion;
using InGame.Event;

namespace Unit
{
    [Serializable]
    public class HitMotionInfo
    {
        public AnimationClip clip;
        public string motionName;
        public float motionDuration;

        public Direction direction;

        public enum Direction
        {
            Front,
            Back
        }
    }

    public class PlayerHitState : PlayerStateBase
    {
        public HitMotionInfo[] hitMotionInfos;

        public override StateType GetStateType() => StateType.Hit;

        protected override StatePriorityType Priority => StatePriorityType.Event;

        private const float hitMotionDuration = 1f;
        private int hitEndTick;

        private HitInfo currentHitInfo;

        private Vector3 currentHitMove;
        private float hitMoveSpeed = 120f;

        private int hitLookAtEndTick = 0;
        private const float HIT_LOOKAT_TIME = .15f;
        private const float CURV_SPEED = 10;

        protected override void SetInfo(INetworkStruct info) => currentHitInfo = ((StateInfo)info).hitInfo;

        #region FSM State
        //EnterState
        protected override void EnterStateAuthority(int enterTick)
        {
            hitLookAtEndTick = Runner.Tick + Mathf.RoundToInt(HIT_LOOKAT_TIME * Runner.TickRate);
        }

        protected override void EnterStateShared(int enterTick)
        {
            HitMotionInfo currentMotionInfo = null;

            Vector3 attackerDir = (currentHitInfo.attackerPos - player.transform.position).normalized;
            float dot = Vector3.Dot(player.transform.forward, attackerDir);
            bool front = dot > 0f;

            Func<HitMotionInfo, bool> directionPredicate =
                front?
                (x => x.direction == HitMotionInfo.Direction.Front) :
                (x => x.direction == HitMotionInfo.Direction.Back);

            var animInfoList = hitMotionInfos.Where(directionPredicate).ToList();
            currentMotionInfo = animInfoList[UnityEngine.Random.Range(0, animInfoList.Count)];
            hitEndTick = Runner.Tick + Mathf.RoundToInt(hitMotionDuration * Runner.TickRate);
            
            PlayAnim(currentMotionInfo.motionName, 0, enterTick);
        }

        //OnState
        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (fsm.input.WasPressed(x => x.skill))
            {
                fsm.SetState<PlayerRoarState>(default, TransitionType.Request);
                return;
            }

            if (Runner.Tick >= hitEndTick)
            {
                fsm.SetState<PlayerMovementState>(default, TransitionType.System);
                return;
            }

            if (Runner.Tick < hitLookAtEndTick)
            {
                Vector3 dir = (currentHitInfo.attackerPos - player.transform.position).normalized;
                SetLookRotation(Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(dir), CURV_SPEED * Runner.DeltaTime));
            }

            Move(currentHitMove * hitMoveSpeed * Runner.DeltaTime);
        }

        //AnimEvent
        protected override void OnAnimEvent(AnimationEventData data)
        {
            switch (data)
            {
                case PlayerMoveAnimEventData moveData:
                    OnHitMove(moveData);
                    break;
            }
        }
        #endregion

        private void OnHitMove(PlayerMoveAnimEventData moveData)
        {
            if (HasStateAuthority == false) return;

            if (moveData.MoveDir == Vector3.zero)
            {
                currentHitMove = Vector3.zero;
                return;
            }

            Vector3 forward = player.transform.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            Vector3 rawMove = (forward * moveData.MoveDir.z + right * moveData.MoveDir.x).normalized;
            Vector3 worldMoveDir = rawMove * moveData.MoveDir.magnitude;

            hitMoveSpeed = moveData.MoveSpeed;
            currentHitMove = worldMoveDir;
        }
    }
}