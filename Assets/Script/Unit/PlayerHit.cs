using Physics;
using System;
using System.Collections;
using UnityEngine;

namespace Unit
{
    public class PlayerHit : MonoBehaviour
    {
        [SerializeField] private HitBox hitBox;
        private Func<bool> isServerLocal;
        private PlayerState playerState;

        private Action<string, float> playerHitMotionSync;

        private Action stopAttackMotion;

        private IEnumerator hitMotionHandle;

        public void Initialize(Func<bool> isServerLocal,PlayerState playerState, Action<string, float> onHitMotionSync, Action stopAttackMotion)
        {
            this.isServerLocal = isServerLocal;
            this.playerState = playerState;
            this.stopAttackMotion = stopAttackMotion;
            this.playerHitMotionSync = onHitMotionSync;

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
            if (isServerLocal.Invoke() == false) return;

            //ToDo
            //HitInfo의 따라 다양한 모션 구사

            playerHitMotionSync?.Invoke("_HitF", 1);
        }
    }
}