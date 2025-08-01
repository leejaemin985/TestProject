using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;

namespace Unit
{
    [Serializable]
    public class AttackMotionInfo
    {
        public AnimationClip clip;
        public string motionName;
        public float motionDuration;

        public float damage;
        public float weight;
        public AttackType attackType;

        public List<AttackBoxActiveInfo> attackBoxTimings;
    }

    [Serializable]
    public class AttackBoxActiveInfo
    {
        public int startTick;
        public int endTick;
    }

    public class PlayerAttackState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Attack;

        private AttackInfo currentAttackMotionInfo;

        [SerializeField] private AttackMotionInfo[] attackMotionInfos;

        [SerializeField] private AttackMotionInfo dashAttackMotionInfo;

        [SerializeField] private float attackTryWindowTime = .1f;

        [Networked] public int currentMotionIndex { get; set; }
        [Networked] public int currentMotionStartTick { get; set; }

        private int attackEndTick;
        private int attackRetryTick;
        private int currentCombo;

        private const float ATTACK_MOVE_DISTANCE_OFFSET = .7f;
        private const float ATTACK_MOVE_RATIO_CLAMP_MAX = 1.2f;

        private Vector3 currentAttackMove;
        private float attackMoveSpeed = 65f;


        private const float COMBO_DELAY_TIME = 1f;
        private IEnumerator comboDelayHandle = null;

        private IEnumerator ComboDelay(float sec)
        {
            yield return new WaitForSeconds(sec);
            currentCombo = 0;
        }

        protected override void SetInfo(INetworkStruct info) => currentAttackMotionInfo = (AttackInfo)info;

        protected override void EnterState(bool sync = true)
        {
            AttackMotionInfo currentMotion = null;
            if (currentAttackMotionInfo.attackMotionType == AttackMotionType.None)
            {
                currentMotionIndex = currentCombo % attackMotionInfos.Length;
                currentMotionStartTick = Runner.Tick;

                currentMotion = attackMotionInfos[currentMotionIndex];
                currentCombo++;

                if (comboDelayHandle != null) StopCoroutine(comboDelayHandle);
                StartCoroutine(comboDelayHandle = ComboDelay(currentMotion.motionDuration + COMBO_DELAY_TIME));
            }
            else if (currentAttackMotionInfo.attackMotionType == AttackMotionType.Dash)
            {
                currentMotion = dashAttackMotionInfo;
            }

            float tickRate = 1 / Runner.DeltaTime;
            attackEndTick = Runner.Tick + Mathf.RoundToInt(currentMotion.motionDuration * tickRate);
            attackRetryTick = attackEndTick - Mathf.RoundToInt(attackTryWindowTime * tickRate);

            PlayAnim(currentMotion.motionName, .1f, sync);


            currentAttackMove = Vector3.zero;

            var enemy = FindEnemy();
            if (enemy != null)
            {
                var dir = (enemy.transform.position - player.transform.position).normalized;
                cc.SetLookRotation(Quaternion.LookRotation(dir));
            }
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;
            if (attackRetryTick <= Runner.Tick && fsm.input.IsSet(x => x.attack)) 
            {
                fsm.SetState<PlayerAttackState>();
                return;
            }

            if (attackEndTick <= Runner.Tick)
            {
                fsm.SetState<PlayerMovementState>();
            }

            cc.Move(currentAttackMove * attackMoveSpeed * Runner.DeltaTime);
        }

        protected override void ExitState()
        {
            weap.SetCollisionActive(false);
        }

        private Player FindEnemy()
        {
            return PlayerRegistry.Instance.RegistedUsers.FirstOrDefault(x => x.Key.Equals(Object.InputAuthority) == false).Value;
        }


        protected override void OnAnimEvent(string param)
        {
            var parts = param.Split("//");
            switch (parts[0])
            {
                case "AttackMove":
                    OnAttackMove(parts[1]);
                    break;
            }
        }

        private void OnAttackMove(string param)
        {
            if (string.IsNullOrEmpty(param) || param.Equals("0"))
            {
                currentAttackMove = Vector3.zero;
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
                float moveRatio = 1f;

                var enemy = FindEnemy();
                if (enemy != null)
                {
                    Vector3 toEnemy = (enemy.transform.position - player.transform.position);
                    toEnemy.y = 0;
                    toEnemy.Normalize();
                
                    forward = toEnemy.normalized;
                    moveRatio = Mathf.Clamp((toEnemy.magnitude - ATTACK_MOVE_DISTANCE_OFFSET) / 1f, 0, ATTACK_MOVE_RATIO_CLAMP_MAX);
                }
                Vector3 right = Vector3.Cross(Vector3.up, forward);

                Vector3 rawMove = (forward * input.z + right * input.x).normalized;
                Vector3 worldMoveDir = rawMove * input.magnitude;

                currentAttackMove = worldMoveDir * moveRatio;
            }
        }

        protected override void OnMasterTick()
        {
            var currentMotion = attackMotionInfos[currentMotionIndex];
            foreach (var timing in currentMotion.attackBoxTimings)
            {
                float startTime = timing.startTick / currentMotion.clip.frameRate;
                float endTime = timing.endTick / currentMotion.clip.frameRate;

                int startTick = currentMotionStartTick + Mathf.RoundToInt(startTime * Runner.TickRate);
                int endTick = currentMotionStartTick + Mathf.RoundToInt(endTime * Runner.TickRate);

                bool active = Runner.Tick > startTick && Runner.Tick < endTick;
                if (active != weap.collisionActive)
                {
                    if (active)
                    {
                        weap.SetCollisionActive(true);
                        weap.SetHitInfo(new()
                        {
                            damaged = attackMotionInfos[currentMotionIndex].damage,
                            weight = attackMotionInfos[currentMotionIndex].weight,
                            attackType = attackMotionInfos[currentMotionIndex].attackType,
                            attackerPos = player.transform.position
                        });
                    }
                    else
                    {
                        weap.SetCollisionActive(false);
                    }
                }
            }

        }
    }
}