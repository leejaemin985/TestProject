using Physics;
using System;
using System.Collections;
using UnityEngine;

namespace Unit
{
    public class PlayerHit : MonoBehaviour
    {
        [SerializeField] private HitBox hitBox;
        private Func<bool> hasInputAhtority;
        private PlayerState playerState;

        private Action<string, float> playerHitMotionSync;

        private Action stopAttackMotion;
        private Action<bool> setHitImmuneFlagEvent;
        private Func<int> getCurrAttackTick;

        private IEnumerator hitMotionHandle;

        public void Initialize(Func<bool> hasInputAuthority,PlayerState playerState, Action<string, float> onHitMotionSync, Action stopAttackMotion, Action<bool> setHitImmuneFlag, Func<int> getCurrAttackTick)
        {
            this.hasInputAhtority = hasInputAuthority;
            this.playerState = playerState;
            this.stopAttackMotion = stopAttackMotion;
            this.playerHitMotionSync = onHitMotionSync;
            this.setHitImmuneFlagEvent = setHitImmuneFlag;
            this.getCurrAttackTick = getCurrAttackTick;

            hitBox.Initialize(PlayerHitEvent);
            hitBox.SetActive(true);
        }

        public PhysicsObject GetPhysicsBox() => hitBox;

        private void SetState()
        {
            playerState.isHit.AddStateOnListener(OnHitState);
            playerState.isHit.AddStateOffListener(OffHitState);
        }

        private void OnHitState()
        {
            playerState.isMotion.state = true;
            stopAttackMotion?.Invoke();
        }

        private void OffHitState()
        {
            playerState.isMotion.state = false;
        }

        public void OnHitMotion(float motionActiveTime)
        {
            playerState.isHit.state = true;
            if (hitMotionHandle != null) StopCoroutine(hitMotionHandle);
            StartCoroutine(hitMotionHandle = RunHitMotion(motionActiveTime));
        }

        private IEnumerator RunHitMotion(float sec)
        {
            yield return new WaitForSeconds(sec);
            playerState.isHit.state = false;
        }

        private void PlayerHitEvent(HitInfo hitInfo)
        {
            //ToDo
            //HitInfo의 따라 다양한 모션 구사
            if (hasInputAhtority.Invoke() == false)
            {
                if (playerState.isAttack.state &&
                    getCurrAttackTick.Invoke() > hitInfo.attackTick)
                {
                    Debug.Log($"Test - Set Off AttackCollision Because you Attack tick is higher");
                    setHitImmuneFlagEvent?.Invoke(false);
                }
            }
            else
            {
                playerHitMotionSync?.Invoke("_HitF", 1);
            }
        }
    }
}