using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unit
{
    public class PlayerDefense : MonoBehaviour
    {
        private PlayerState playerState;
        private Action<Vector3> onMoveAction;
        private Action<string, PlayerAnimController.PlayerAnimLayer, float> animEvent;

        private const float DEFENSEHIT_MOTION_TIME = .5f;
        private WaitForSeconds defenseHitRunMotionWaitTime = new WaitForSeconds(DEFENSEHIT_MOTION_TIME);

        public void Initialize(PlayerState playerState, Action<Vector3> onMoveAction, Action<string, PlayerAnimController.PlayerAnimLayer, float> animEvent)
        {
            this.playerState = playerState;
            this.onMoveAction = onMoveAction;
            this.animEvent = animEvent;

            SetState();
        }

        public bool canDefense => !playerState.isMotion.state;

        private void SetState()
        {
            playerState.isDefense.AddStateOnListener(OnDefenseState);
            playerState.isDefense.AddStateOffListener(OffDefenseState);

            playerState.isDefenseHit.AddStateOnListener(OnDefenseHitState);
            playerState.isDefenseHit.AddStateOffListener(OffDefenseHitState);
        }

        private void OnDefenseState() { }

        private void OffDefenseState() { }

        private void OnDefenseHitState()
        {
            playerState.isMotion.state = true;
            playerState.isDefense.state = true;

            onMoveAction?.Invoke(Vector3.zero);
        }

        private void OffDefenseHitState()
        {
            playerState.isMotion.state = false;
            playerState.isDefense.state = false;

        }

        public bool TrySetDefense(bool set)
        {
            if (set) return canDefense;
            return true;
        }

        public void SetDefense(bool set)
        {
            playerState.isDefense.state = set;
        }

        public void SetDefenseHit()
        {
            playerState.isDefenseHit.state = true;
            animEvent?.Invoke("_DefenseHit_1", PlayerAnimController.PlayerAnimLayer.DEFENSE, 0);

            if (defenseHitRunMotionHandle != null) StopCoroutine(defenseHitRunMotionHandle);
            StartCoroutine(defenseHitRunMotionHandle = RunDefenseHitMotion());
        }

        private IEnumerator defenseHitRunMotionHandle = null;
        private IEnumerator RunDefenseHitMotion()
        {
            yield return defenseHitRunMotionWaitTime;
            playerState.isDefenseHit.state = false;
            animEvent?.Invoke("_Defense", PlayerAnimController.PlayerAnimLayer.DEFENSE, 0);
        }
    }
}