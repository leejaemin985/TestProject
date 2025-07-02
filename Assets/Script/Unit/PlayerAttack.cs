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
        private Func<bool> hasInputAuthority;
        private PlayerState playerState;
        private Action<string> SetAttackAnimEvent;

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

        public void Initialized(Func<bool> hasAuthority, PlayerState playerState, Action<string> setAttackAnim, Katana weapon, PhysicsObject playerHitBox)
        {
            this.hasInputAuthority = hasAuthority;
            this.playerState = playerState;
            this.SetAttackAnimEvent = setAttackAnim;
            this.weapon = weapon;
            this.weapon.Initialize(HitEventListener, playerHitBox);

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

        public bool canAttack => !playerState.isAttack.state && !playerState.isHit.state;

        private int comboNum = 0;

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
            }

            result.success = true;
            return result;
        }

        public void SetAttackMotion(float motionDuration)
        {
            comboNum++;
            playerState.isAttack.state = true;
            if (runAttackMotionHandle != null) StopCoroutine(runAttackMotionHandle);
            StartCoroutine(runAttackMotionHandle = RunAttackMotion(motionDuration));
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
            //피격 연산은 상대측에서 방어 성공여부와 함께 진행합니다.
            if (hasInputAuthority.Invoke() == true) return;
            this.weapon.SetCollisionActive(set);
        }

        public void SetHitInfo(HitInfo[] hitInfos)
        {
            this.currAttackMotionHitInfos = hitInfos;
        }

        public void HitEventListener(CollisionInfos collisionInfo)
        {
            Debug.Log($"Test - collisionEvent: {collisionInfo.collisionInfos[0].hitObject.name}");
            foreach (var info in collisionInfo.collisionInfos)
            {
                info.hitObject.OnHitEvent(currAttackMotionHitInfos[0]);
            }
        }
    }
}