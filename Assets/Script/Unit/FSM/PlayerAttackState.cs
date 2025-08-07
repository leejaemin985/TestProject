using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;
using Fusion.Addons.SimpleKCC;

namespace Unit
{
    public class PlayerAttackState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Attack;


        [SerializeField] private float attackTryWindowTime = .1f;

        private Dictionary<AttackMotionType, List<AttackMotionInfo>> attackMotionInfos;

        [Networked] private int attackMotionStartTick { get; set; }
        [Networked] private int currentMotionIndex { get; set; }
        [Networked] private AttackInfo currentAttackMotionInfo { get; set; }


        private int attackEndTick;
        private int attackRetryTick;
        private int currentCombo;

        private const float ATTACK_MOVE_DISTANCE_OFFSET = .5f;
        private const float ATTACK_MOVE_RATIO_CLAMP_MAX = 2f;

        private Vector3 currentAttackMove;
        private float attackMoveSpeed = 100f;


        private const float COMBO_DELAY_TIME = 1f;
        private IEnumerator comboDelayHandle = null;

        private IEnumerator ComboDelay(float sec)
        {
            yield return new WaitForSeconds(sec);
            currentCombo = 0;
        }

        public override void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator anim, Katana weap)
        {
            base.Initialize(player, fsm, cc, anim, weap);

            attackMotionInfos = new();
            foreach (var type in Enum.GetValues(typeof(AttackMotionType)))
            {
                const string KEY_BASE = "Scriptable/AttackMotionInfos_";
                var scriptableMotionInfos = Resources.Load<PlayerAttackMotionInfoScriptable>($"{KEY_BASE}{type}").attackMotionInfos;

                attackMotionInfos.Add((AttackMotionType)type, scriptableMotionInfos);
            }
        }


        protected override void SetInfo(INetworkStruct info) => currentAttackMotionInfo = (AttackInfo)info;

        protected override void EnterState(bool sync = true)
        {
            var currentMotion = ResolveAttackMotion();

            currentCombo++;
            var motionInfo = currentAttackMotionInfo;
            motionInfo.attackMotionType = AttackMotionType.None;
            currentAttackMotionInfo = motionInfo;

            if (comboDelayHandle != null) StopCoroutine(comboDelayHandle);
            StartCoroutine(comboDelayHandle = ComboDelay(currentMotion.motionDuration + COMBO_DELAY_TIME));


            float tickRate = 1 / Runner.DeltaTime;
            attackEndTick = Runner.Tick + Mathf.RoundToInt(currentMotion.motionDuration * tickRate);
            attackRetryTick = attackEndTick - Mathf.RoundToInt(attackTryWindowTime * tickRate);

            attackMotionStartTick = Runner.Tick;
            PlayAnim(currentMotion.motionName, .1f, sync);


            currentAttackMove = Vector3.zero;

            var enemy = FindEnemy();
            if (enemy != null)
            {
                var dir = (enemy.transform.position - player.transform.position).normalized;
                cc.SetLookRotation(Quaternion.LookRotation(dir));
            }
        }

        private AttackMotionInfo ResolveAttackMotion()
        {
            var targetList = attackMotionInfos[currentAttackMotionInfo.attackMotionType];
            currentMotionIndex = currentCombo % targetList.Count;
            return targetList[currentMotionIndex];
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;
            if (attackRetryTick <= Runner.Tick && fsm.input.IsSet(x => x.attack))
            {
                fsm.SetState<PlayerAttackState>();
                return;
            }

            if (attackEndTick <= Runner.Tick)
            {
                fsm.SetState<PlayerMovementState>();
            }

            cc.Move(currentAttackMove * attackMoveSpeed * Runner.DeltaTime);
        }

        protected override void ExitState()
        {
            weap.SetCollisionActive(false);
        }

        private Player FindEnemy()
        {
            return PlayerRegistry.Instance.RegistedUsers.FirstOrDefault(x => x.Key.Equals(Object.InputAuthority) == false).Value;
        }

        protected override void OnExitRender()
        {
            weap.SetSlashParticleActive(false);
        }

        protected override void OnMasterTick()
        {
            var currentMotion = ResolveAttackMotion();
            foreach (var timing in currentMotion.attackTimings)
            {
                int startTick = attackMotionStartTick + ConvertFrameToTick(timing.startTick, currentMotion.clip, Runner.TickRate);
                int endTick = attackMotionStartTick + ConvertFrameToTick(timing.endTick, currentMotion.clip, Runner.TickRate);

                bool active = Runner.Tick >= startTick && Runner.Tick <= endTick;
                Debug.Log($"Test - active: {active}");

                if (active != weap.collisionActive)
                {
                    if (active)
                    {
                        weap.SetCollisionActive(true);
                        weap.SetHitInfo(new()
                        {
                            damaged = currentMotion.damage,
                            weight = currentMotion.weight,
                            attackType = currentMotion.attackType,
                            attackerPos = cc.transform.position
                        });
                    }
                    else
                    {
                        weap.SetCollisionActive(false);
                    }
                }
            }
        }

        private int ConvertFrameToTick(int frame, AnimationClip clip, float tickRate)
        {
            float sec = frame / clip.frameRate;
            return Mathf.RoundToInt(sec * tickRate);
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
                    SetWeapCollision(parts[1]);
                    break;
                case "SetSlashParticleEffect":
                    SetSlashParticleAcitve(parts[1]);
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

                Vector3 forward = player.transform.forward;
                float moveRatio = 1f;

                var enemy = FindEnemy();
                if (enemy != null)
                {
                    Vector3 toEnemy = (enemy.transform.position - player.transform.position);
                    toEnemy.y = 0;
                    toEnemy.Normalize();

                    forward = toEnemy.normalized;
                    moveRatio = Mathf.Clamp((toEnemy.magnitude - ATTACK_MOVE_DISTANCE_OFFSET) / 1f, 0, ATTACK_MOVE_RATIO_CLAMP_MAX);
                }
                Vector3 right = Vector3.Cross(Vector3.up, forward);

                Vector3 rawMove = (forward * input.z + right * input.x).normalized;
                Vector3 worldMoveDir = rawMove * input.magnitude;

                currentAttackMove = worldMoveDir * moveRatio;
            }
        }

        private void SetWeapCollision(string param)
        {
            return;
            weap.SetCollisionActive(param.Equals("0") == false);
            var currentMotion = ResolveAttackMotion();
            weap.SetHitInfo(new()
            {
                damaged = currentMotion.damage,
                weight = currentMotion.weight,
                attackType = currentMotion.attackType,
                attackerPos = cc.transform.position
            });
        }

        private void SetSlashParticleAcitve(string param)
        {
            weap.SetSlashParticleActive(param.Equals("0") == false);
        }
    }
}