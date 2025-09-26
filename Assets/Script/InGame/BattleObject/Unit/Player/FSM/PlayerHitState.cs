using System;
using System.Linq;

using UnityEngine;

using Fusion;

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

        protected override void SetInfo(INetworkStruct info) => currentHitInfo = ((StateInfo)info).hitInfo;

        #region FSM State
        //EnterState
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

            Move(currentHitMove * hitMoveSpeed * Runner.DeltaTime);
        }

        //AnimEvent
        protected override void OnAnimEvent(string param)
        {
            var parts = param.Split("//");
            switch (parts[0])
            {
                case "HitMove":
                    OnHitMove(parts[1]);
                    break;
            }
        }
        #endregion

        private void OnHitMove(string param)
        {
            if (HasStateAuthority == false) return;

            if (string.IsNullOrEmpty(param) || param.Equals("0"))
            {
                currentHitMove = Vector3.zero;
                return;
            }

            string[] moveVector = param
                .Split(',')
                .Select(p => p.Trim())
                .ToArray();

            if (moveVector.Length == 3 &&
                float.TryParse(moveVector[0], out float x) &&
                float.TryParse(moveVector[1], out float y) &&
                float.TryParse(moveVector[2], out float z))
            {
                Vector3 input = new Vector3(x, y, z);

                Vector3 forward = player.transform.forward;
                Vector3 right = Vector3.Cross(Vector3.up, forward);

                Vector3 rawMove = (forward * input.z + right * input.x).normalized;
                Vector3 worldMoveDir = rawMove * input.magnitude;

                currentHitMove = worldMoveDir;
            }
        }
    }
}