using Fusion;
using UnityEngine;

namespace Unit
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimController : MonoBehaviour
    {
        public enum PlayerAnimLayer
        {
            IDLE = 0,
            ATTACK = 1,
            HIT = 2
        }

        private const string KEYNAME_MOVE_HORIZONTAL = "_Horizontal";
        private const string KEYNAME_MOVE_VERTICAL = "_Vertical";
        private const string KEYNAME_RUN_WEIGHT = "_RunWeight";

        private Animator anim;

        private PlayerState playerState;

        private Vector2 targetMoveAnimDir = default;
        private Vector2 moveAnimDir = default;
        private float runAnimWeight = default;
        private float curveSpeed = 10f;

        private bool initialized = false;

        public void Initialize(PlayerState playerState)
        {
            this.playerState = playerState;
            anim = GetComponent<Animator>();

            initialized = true;
        }

        private float attackMotionLayerWeight = 0f;
        private float hitMotionLayerWeight = 0f;

        public void Update()
        {
            if (initialized == false) return;

            moveAnimDir = Vector2.Lerp(moveAnimDir, targetMoveAnimDir, curveSpeed * Time.deltaTime);
            anim.SetFloat(KEYNAME_MOVE_HORIZONTAL, moveAnimDir.x);
            anim.SetFloat(KEYNAME_MOVE_VERTICAL, moveAnimDir.y);
            anim.SetFloat(KEYNAME_RUN_WEIGHT, runAnimWeight);

            SetAttackMotionWeight();
            SetHitMotionWeight();
        }

        public void SetMoveAnimDirection(Vector2 dir, float runWeight)
        {
            targetMoveAnimDir = new Vector2(dir.x, dir.y).normalized;
            runAnimWeight = runWeight;
        }

        public void Play(string animName) => anim.Play(animName, 0, 0);

        public void Play(string animName, PlayerAnimLayer layerIndex, float latency)
        {
            anim.CrossFadeInFixedTime(animName, 0.05f, (int)layerIndex, latency);
        }

        private void SetAttackMotionWeight()
        {
            float targetWeight = 1;
            float lerpSpeed = .1f;
            if (playerState.isAttack.state)
            {
                targetWeight = 1;
                lerpSpeed = .2f;
            }
            else
            {
                targetWeight = playerState.isMotion.state ? 1 : 0;
                lerpSpeed = .2f;
            }

            attackMotionLayerWeight = Mathf.Lerp(attackMotionLayerWeight, targetWeight, lerpSpeed);
            anim.SetLayerWeight(anim.GetLayerIndex("AttackMotion"), attackMotionLayerWeight);
        }

        private void SetHitMotionWeight()
        {
            float targetWeight = 1;
            float lerpSpeed = .1f;
            if (playerState.isHit.state)
            {
                targetWeight = 1f;
                lerpSpeed = .2f;
            }
            else
            {
                targetWeight = 0f;
                lerpSpeed = .2f;
            }

            hitMotionLayerWeight = Mathf.Lerp(hitMotionLayerWeight, targetWeight, lerpSpeed);
            anim.SetLayerWeight(anim.GetLayerIndex("HitMotion"), hitMotionLayerWeight);
        }
    }
}