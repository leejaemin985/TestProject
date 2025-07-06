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
        private Func<Transform> getPlayerTransform;
        private PlayerState playerState;
        private Action<Vector3> onMoveAction;
        private Action<Quaternion> SetRotAction;
        private Func<Player> FindEnemyUnit;

        private Katana weapon;
        private Vector3 attackMoveDir;

        public struct AttackMotion
        {
            public bool success;

            public string motionName;
            public float duration;
            public float motionActiveTime;

            public HitInfo hitInfo;
        }

        public void Initialized(
            Func<bool> isServerLocal,
            Func<Transform> getPlayerTransform,
            PlayerState playerState,
            Katana weapon,
            PhysicsObject playerHitBox,
            Action<Vector3> onMoveAction,
            Action<Quaternion> setRotAction,
            Func<Player> findEnemyUnit)
        {
            this.isServerLocal = isServerLocal;
            this.getPlayerTransform = getPlayerTransform;
            this.playerState = playerState;
            this.weapon = weapon;
            this.weapon.Initialize(playerHitBox, () => canHit, HitEventListener);
            this.onMoveAction = onMoveAction;
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

        public void SetAttackMoveDir(Vector3 input)
        {
            this.attackMoveDir = input;

            Player enemy = FindEnemyUnit.Invoke();
            if (enemy == null) return;

            Transform enemyTransform = enemy.transform;
            Transform playerTransform = getPlayerTransform.Invoke();

            var toEnemy = (enemyTransform.position - playerTransform.position);

            Vector3 toEnemyForward = toEnemy.normalized;
            Vector3 toEnemyRight = Vector3.Cross(Vector3.up, toEnemyForward);

            float distance = toEnemy.magnitude;
            float moveRatio = Mathf.Clamp((distance - 0.5f) / 1f, 0, 2);

            Vector3 worldMoveDir = (toEnemyForward * attackMoveDir.z + toEnemyRight * attackMoveDir.x).normalized;

            onMoveAction?.Invoke(worldMoveDir * moveRatio);
            attackMoveDir = Vector2.zero;
        }

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

            // LookAt Enemy
            var detectedEnemy = FindEnemyUnit.Invoke();
            if (detectedEnemy != null)
            {
                var dir = (detectedEnemy.transform.position - transform.position);
                dir.y = 0;
                if (dir.sqrMagnitude > 0.0001f)
                    SetRotAction?.Invoke(Quaternion.LookRotation(dir.normalized));
            }


            result.motionName = $"{ATTACK_NAME_BASE}_{currentCombo}";
            result.motionActiveTime = attackMotionInfos[currentCombo].duration;
            result.hitInfo = attackMotionInfos[currentCombo].hitInfo;

            result.success = true;
            return result;
        }

        public void SetAttackMotion(float motionDuration)
        {
            playerState.isAttack.state = true;

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
            //서버 로컬에 있는 플레이어만 피격판정을 연산합니다.
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