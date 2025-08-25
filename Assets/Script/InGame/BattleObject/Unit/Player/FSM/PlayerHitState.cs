using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using Fusion;
using System.Linq;

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

        [SerializeField] private GameObject bloodEffectObject;
        private ParticleSystem bloodEffect;

        public override StateType GetStateType() => StateType.Hit;

        public const float hitMotionDuration = 1f;
        private int hitEndTick;

        private HitInfo currentHitInfo;

        private Vector3 currentHitMove;
        private float hitMoveSpeed = 120f;

        protected override void SetInfo(INetworkStruct info)
        {
            HitInfo hitInfo = (HitInfo)info;
            currentHitInfo = hitInfo;
        }

        protected override void OnEnterRender()
        {
            if (bloodEffect == null) bloodEffect = bloodEffectObject.GetComponent<ParticleSystem>();
            bloodEffect.Play();
        }

        protected override void EnterState(bool sync = true)
        {
            base.EnterState();

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
            PlayAnim(currentMotionInfo.motionName, 0, sync);
            
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (Runner.Tick >= hitEndTick)
            {
                fsm.SetState<PlayerMovementState>();
                return;
            }

            cc.Move(currentHitMove * hitMoveSpeed * Runner.DeltaTime);
        }

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

        private void OnHitMove(string param)
        {
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