using JetBrains.Annotations;
using Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit;
using UnityEngine;

namespace Unit
{
    [Serializable]
    public class MotionClipInfo
    {
        public AnimationClip clip;
        public float duration;
    }

    public class PlayerAttack : MonoBehaviour
    {
        private Func<bool> isServerLocal;
        private PlayerState playerState;
        private Action<string> SetAttackAnimEvent;
        private Action<Quaternion> SetRotAction;
        private Func<Player> FindEnemyUnit;

        private Katana weapon;

        private HitInfo[] currAttackMotionHitInfos;

        public struct AttackMotion
        {
            public string motionName;
            public float duration;
            public float motionActiveTime;
            public bool success;

            public HitInfo[] hitInfos;
        }

        public void Initialized(Func<bool> isServerLocal, PlayerState playerState, Action<string> setAttackAnim, Katana weapon, PhysicsObject playerHitBox, Action<Quaternion> setRotAction, Func<Player> findEnemyUnit)
        {
            this.isServerLocal = isServerLocal;
            this.playerState = playerState;
            this.SetAttackAnimEvent = setAttackAnim;
            this.weapon = weapon;
            this.weapon.Initialize(HitEventListener, playerHitBox);
            this.SetRotAction = setRotAction;
            this.FindEnemyUnit = findEnemyUnit;

            SetState();
        }

        private void SetState()
        {
            playerState.isAttack.AddStateOnListener(OnAttackState);
            playerState.isAttack.AddStateOffListener(OffAttackState);

        }

        private void OnAttackState()
        {
            playerState.isMotion.state = true;
        }

        private void OffAttackState()
        {
            this.weapon.SetCollisionActive(false);
        }

        /// <summary>
        /// 어택모션중 다음 어택모션과 부드럽게 연계하기 위해 중간 버퍼 딜레이가 있습니다. 
        /// 이를 무시하고 attack, motion 둘다 false로 하기 위해선 attackMotion을 중지시킬때 해당 함수를 호출합니다.
        /// </summary>
        public void StopAttackMotion()
        {
            playerState.isAttack.state = false;
            playerState.isMotion.state = false;
        }

        private const string LIGHT_ATTACK_NAME_BASE = "_LightAttack";
        private const string HEAVY_ATTACK_NAME_BASE = "_HeavyAttack";

        public MotionClipInfo[] lightAttackClip;
        public MotionClipInfo[] heavyAttackClip;

        public bool canAttack => !playerState.isAttack.state && !playerState.isHit.state;// && !playerState.isDefense.state;

        private int comboNum = 0;
        private const float COMBO_INPUT_WINDOW_DURATION = .5f;
        private IEnumerator comboWindowHandle = default;

        private IEnumerator runAttackMotionHandle = default;
        private const float ATTACK_INPUT_BUFFER_TIME = .2f;
        private WaitForSeconds attackInputBufferDelay = new WaitForSeconds(ATTACK_INPUT_BUFFER_TIME);

        public AttackMotion TryAttack()
        {
            AttackMotion result = new();
            if (!canAttack)
            {
                result.success = false;
                return result;
            }

            comboNum %= lightAttackClip.Length;
            result.motionName = $"{LIGHT_ATTACK_NAME_BASE}_{comboNum}";
            result.motionActiveTime = lightAttackClip[comboNum].duration;

            result.hitInfos = new HitInfo[1] { new()
                {
                    damaged = 1,
                    weight = 1,
                    attackType = AttackType.GENERIC
                }};

            result.success = true;
            return result;
        }

        public void SetAttackMotion(float motionDuration)
        {
            comboNum++;
            playerState.isAttack.state = true;

            // LookAt Enemy
            var detectedEnemy = FindEnemyUnit.Invoke();
            if (detectedEnemy != null)
            {
                var dir = (detectedEnemy.transform.position - transform.position);
                dir.y = 0;
                if (dir.sqrMagnitude > 0.0001f)
                    SetRotAction?.Invoke(Quaternion.LookRotation(dir.normalized));
            }

            if (runAttackMotionHandle != null) StopCoroutine(runAttackMotionHandle);
            StartCoroutine(runAttackMotionHandle = RunAttackMotion(motionDuration));

            if (comboWindowHandle != null) StopCoroutine(comboWindowHandle);
            StartCoroutine(comboWindowHandle = ComboWindowCounter(motionDuration));
        }

        private IEnumerator ComboWindowCounter(float motionDuration)
        {
            yield return new WaitForSeconds(motionDuration + COMBO_INPUT_WINDOW_DURATION);
            comboNum = 0;
        }

        private IEnumerator RunAttackMotion(float sec)
        {
            yield return new WaitForSeconds(sec - ATTACK_INPUT_BUFFER_TIME);
            playerState.isAttack.state = false;
            yield return attackInputBufferDelay;

            playerState.isMotion.state = false;
        }

        public void SetWeaponActive(bool set)
        {
            if (isServerLocal.Invoke() == false) return;
            this.weapon.SetCollisionActive(set);
        }

        public void SetHitInfo(HitInfo[] hitInfos)
        {
            this.currAttackMotionHitInfos = hitInfos;
        }

        public void HitEventListener(CollisionInfos collisionInfo)
        {
            if (playerState.isHit.state) return;
            foreach (var info in collisionInfo.collisionInfos)
            {
                info.hitObject.OnHitEvent(currAttackMotionHitInfos[0]);
            }
        }
    }
}