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
        public HitInfo hitInfo;
    }

    public class PlayerAttack : MonoBehaviour
    {
        private Func<bool> isServerLocal;
        private PlayerState playerState;
        private Action<string> SetAttackAnimEvent;
        private Action<Quaternion> SetRotAction;
        private Func<Player> FindEnemyUnit;

        private Katana weapon;

        public struct AttackMotion
        {
            public bool success;

            public string motionName;
            public float duration;
            public float motionActiveTime;

            public HitInfo hitInfo;
        }

        public void Initialized(Func<bool> isServerLocal, PlayerState playerState, Action<string> setAttackAnim, Katana weapon, PhysicsObject playerHitBox, Action<Quaternion> setRotAction, Func<Player> findEnemyUnit)
        {
            this.isServerLocal = isServerLocal;
            this.playerState = playerState;
            this.SetAttackAnimEvent = setAttackAnim;
            this.weapon = weapon;
            this.weapon.Initialize(playerHitBox, () => canHit, HitEventListener);
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
        /// ���ø���� ���� ���ø�ǰ� �ε巴�� �����ϱ� ���� �߰� ���� �����̰� �ֽ��ϴ�. 
        /// �̸� �����ϰ� attack, motion �Ѵ� false�� �ϱ� ���ؼ� attackMotion�� ������ų�� �ش� �Լ��� ȣ���մϴ�.
        /// </summary>
        public void StopAttackMotion()
        {
            if (playerState.isAttack.state == false) return;

            if (runAttackMotionHandle != null) StopCoroutine(runAttackMotionHandle);
            if (comboWindowHandle != null) StopCoroutine(comboWindowHandle);
            comboNum = 0;

            playerState.isAttack.state = false;
            playerState.isMotion.state = false;
        }

        private const string ATTACK_NAME_BASE = "_LightAttack";

        public MotionClipInfo[] attackMotionInfos;

        private bool canAttack => !playerState.isAttack.state && !playerState.isHit.state;

        private bool canHit => !playerState.isHit.state;

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

            int currentCombo = comboNum % attackMotionInfos.Length;
            comboNum++;

            result.motionName = $"{ATTACK_NAME_BASE}_{currentCombo}";
            result.motionActiveTime = attackMotionInfos[currentCombo].duration;
            result.hitInfo = attackMotionInfos[currentCombo].hitInfo;

            result.success = true;
            return result;
        }

        public void SetAttackMotion(float motionDuration)
        {
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
            //���� ���ÿ� �ִ� �÷��̾ �ǰ������� �����մϴ�.
            if (isServerLocal.Invoke() == false) return;
            this.weapon.SetCollisionActive(set);
        }

        public void SetHitInfo(HitInfo hitInfo)
        {
            if (isServerLocal.Invoke() == false) return;
            this.weapon.SetHitInfo(hitInfo);
        }

        public void HitEventListener(CollisionInfos collisionInfo)
        {
            //HitEvent Callback
        }
    }
}