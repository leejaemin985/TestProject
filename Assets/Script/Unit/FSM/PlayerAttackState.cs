using System;
using System.Linq;
using UnityEngine;

namespace Unit
{
    [Serializable]
    public class AttackMotionInfo
    {
        public AnimationClip clip;
        public string motionName;
        public float motionDuration;
    }

    public class PlayerAttackState : PlayerStateBase
    {
        [SerializeField] AttackMotionInfo[] attackMotionInfos;

        [SerializeField] private float attackTryWindowTime = .1f;

        private int attackEndTick;
        private int attackRetryTick;
        private int currentCombo;

        private const float ATTACK_MOVE_DISTANCE_OFFSET = .7f;
        private const float ATTACK_MOVE_RATIO_CLAMP_MAX = 1.2f;

        private Vector3 currentAttackMove;
        private float attackMoveSpeed = 65f;

        protected override void EnterState()
        {
            var currentMotion = attackMotionInfos[currentCombo % attackMotionInfos.Length];
            currentCombo++;

            currentAttackMove = Vector3.zero;

            float tickRate = 1 / fsm.deltaTime;
            attackEndTick = fsm.cachedTick + Mathf.RoundToInt(currentMotion.motionDuration * tickRate);
            attackRetryTick = attackEndTick - Mathf.RoundToInt(attackTryWindowTime * tickRate);

            fsm.RPC_RunMotion(currentMotion.motionName, fsm.cachedTick, 0);

            var enemy = FindEnemy();
            if (enemy != null)
            {
                var dir = (enemy.transform.position - fsm.player.transform.position).normalized;
                fsm.cc.SetLookRotation(Quaternion.LookRotation(dir));
            }
        }

        protected override void OnState()
        {
            if (!fsm.HasAuthority) return;
            if (attackRetryTick <= fsm.cachedTick && fsm.input.IsSet(x => x.attack)) 
            {
                fsm.SetState<PlayerAttackState>();
                return;
            }

            if (attackEndTick <= fsm.cachedTick)
            {
                fsm.SetState<PlayerMovementState>();
            }

            fsm.cc.Move(currentAttackMove * attackMoveSpeed * fsm.deltaTime);
        }

        protected override void ExitState()
        {
            currentAttackMove = Vector3.zero;
            fsm.playerWeap.SetCollisionActive(false);
        }

        protected override void OnRender()
        {

        }

        private Player FindEnemy()
        {
            Player enemy = null;
            foreach (var user in PlayerRegistry.Instance.RegistedUsers.Values)
            {
                if (!user.Equals(fsm.player)) enemy = user;
            }
            return enemy;
        }


        protected override void OnAnimEvent(string param)
        {
            var parts = param.Split("//");
            switch (parts[0])
            {
                case "AttackMove":
                    OnAttackMove(parts[1]);
                    break;
                case "SetWeapCollision":
                    SetWeaponCollision(parts[1]);
                    break;
            }
        }

        private void OnAttackMove(string param)
        {
            if (string.IsNullOrEmpty(param) || param.Equals("0"))
            {
                currentAttackMove = Vector3.zero;
                return;
            }
            
            string[] moveVector = param
                .Split(',')
                .Select(p => p.Trim())
                .ToArray();
            
            if (moveVector.Length == 3 &&
                float.TryParse(moveVector[0], out float x) &&
                float.TryParse(moveVector[1], out float y) &&
                float.TryParse(moveVector[2], out float z))
            {
                Vector3 input = new Vector3(x, y, z);

                Vector3 forward = fsm.player.transform.forward;
                float moveRatio = 1f;

                var enemy = FindEnemy();
                if (enemy != null)
                {
                    Vector3 toEnemy = (enemy.transform.position - fsm.player.transform.position);

                    forward = toEnemy.normalized;
                    moveRatio = Mathf.Clamp((toEnemy.magnitude - ATTACK_MOVE_DISTANCE_OFFSET) / 1f, 0, ATTACK_MOVE_RATIO_CLAMP_MAX);
                }
                Vector3 right = Vector3.Cross(Vector3.up, forward);

                Vector3 rawMove = (forward * input.z + right * input.x).normalized;
                Vector3 worldMoveDir = rawMove * input.magnitude;

                currentAttackMove = worldMoveDir * moveRatio;
            }
        }

        private void SetWeaponCollision(string param)
        {
            fsm.playerWeap.SetCollisionActive(string.Equals(param, "0") ? false : true);
        }
    }
}