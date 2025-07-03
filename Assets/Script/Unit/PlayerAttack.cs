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
        public float firstAttackTime;
    }

    public class PlayerAttack : MonoBehaviour
    {
        private Func<bool> hasInputAuthority;
        private PlayerState playerState;
        private Action<string> SetAttackAnimEvent;
        private Action<Quaternion> SetRotAction;
        private Func<Player> FindEnemyUnit;

        private bool hitImmuneFlag = true;
        private int currAttackTick = 0;

        private Katana weapon;

        private HitInfo[] currAttackMotionHitInfos;

        public struct AttackMotion
        {
            public string motionName;
            public float duration;
            public float motionActiveTime;
            public float firstAttackTime;
            public bool success;

            public HitInfo[] hitInfos;
        }

        public void Initialized(Func<bool> hasInputAuthority, PlayerState playerState, Action<string> setAttackAnim, Katana weapon, PhysicsObject playerHitBox, Action<Quaternion> setRotAction, Func<Player> findEnemyUnit)
        {
            this.hasInputAuthority = hasInputAuthority;
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

        public AttackMotion TryAttack(bool isHeavy)
        {
            AttackMotion result = new();
            if (!canAttack)
            {
                result.success = false;
                return result;
            }

            if (isHeavy)
            {
                comboNum %= heavyAttackClip.Length;
                result.motionName = $"{HEAVY_ATTACK_NAME_BASE}_{comboNum}";
                result.motionActiveTime = heavyAttackClip[comboNum].duration;

                result.hitInfos = new HitInfo[1] { new()
                {
                    damaged = 2,
                    weight = 2,
                    attackType = AttackType.GENERIC
                }};
            }
            else
            {
                comboNum %= lightAttackClip.Length;
                result.motionName = $"{LIGHT_ATTACK_NAME_BASE}_{comboNum}";
                result.motionActiveTime = lightAttackClip[comboNum].duration;

                result.hitInfos = new HitInfo[1] { new()
                {
                    damaged = 1,
                    weight = 1,
                    attackType = AttackType.GENERIC
                }};
                result.firstAttackTime = lightAttackClip[comboNum].firstAttackTime;
            }

            result.success = true;
            return result;
        }

        public void SetAttackMotion(float motionDuration, int currAttackTick)
        {
            comboNum++;
            playerState.isAttack.state = true;

            hitImmuneFlag = true;
            this.currAttackTick = currAttackTick;

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

        public void SetHitImmuneFlag(bool set) => hitImmuneFlag = set;

        public void SetWeaponActive(bool set)
        {
            //피격 연산은 상대측에서 방어 성공여부와 함께 진행합니다.
            //if (hasInputAuthority.Invoke() == true) return;
            if (hitImmuneFlag == false)
            {
                Debug.Log($"Test - hit immuneFlag == false");
                return;
            }

            this.weapon.SetCollisionActive(set);
        }

        public void SetHitInfo(HitInfo[] hitInfos)
        {
            this.currAttackMotionHitInfos = hitInfos;
        }

        public void HitEventListener(CollisionInfos collisionInfo)
        {
            foreach (var info in collisionInfo.collisionInfos)
            {
                info.hitObject.OnHitEvent(currAttackMotionHitInfos[0]);
            }
        }

        public int GetCurrAttackTick() => currAttackTick;
    }
}