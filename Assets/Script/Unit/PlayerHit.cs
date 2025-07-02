using Physics;
using System;
using System.Collections;
using UnityEngine;

namespace Unit
{
    public class PlayerHit : MonoBehaviour
    {
        [SerializeField] private HitBox hitBox;
        private PlayerState playerState;
        private Action stopAttackMotion;

        private IEnumerator hitMotionHandle;

        public void Initialize(PlayerState playerState, Action<HitInfo> hitEvent, Action stopAttackMotion)
        {
            this.playerState = playerState;
            this.stopAttackMotion = stopAttackMotion;

            hitBox.Initialize(hitEvent);
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
    }
}