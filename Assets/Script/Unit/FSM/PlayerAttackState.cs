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
    }

    public class PlayerAttackState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Attack;

        private AttackInfo currentAttackMotionInfo;

        [SerializeField] private AttackMotionInfo[] attackMotionInfos;

        [SerializeField] private AttackMotionInfo dashAttackMotionInfo;

        [SerializeField] private float attackTryWindowTime = .1f;

        [Networked] private int currentMotionIndex { get; set; }
        [Networked] private int currentMotionStartTick { get; set; }

        private int attackEndTick;
        private int attackRetryTick;
        private int currentCombo;

        private const float ATTACK_MOVE_DISTANCE_OFFSET = .5f;
        private const float ATTACK_MOVE_RATIO_CLAMP_MAX = 2f;

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
                currentAttackMotionInfo.attackMotionType = AttackMotionType.None;
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
                case "SetWeapCollision":
                    SetWeapCollision(parts[1]);
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

        private void SetWeapCollision(string param)
        {
            weap.SetCollisionActive(param.Equals("0") == false);
        }
    }
}