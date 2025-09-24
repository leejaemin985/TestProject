using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.Networking;

namespace Unit
{
    public class PlayerAttackState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Attack;

        protected override StatePriorityType Priority => StatePriorityType.Free;

        [SerializeField] private float attackTryWindowTime = .1f;

        private Dictionary<AttackMotionType, List<AttackMotionInfo>> attackMotionInfos;

        [Networked] private int currentMotionStartTick { get; set; }
        [Networked] private int currentMotionIndex { get; set; }
        [Networked] private AttackInfo currentMotionInfo { get; set; }

        private AttackMotionInfo currentMotion;

        private int attackEndTick;
        private int attackRetryTick;
        private int currentCombo;


        private const float ATTACK_MOVE_DISTANCE_OFFSET = .5f;
        private const float ATTACK_MOVE_RATIO_CLAMP_MAX = 2f;

        private Vector3 currentAttackMove;
        private float attackMoveSpeed = 100f;


        private const float COMBO_DELAY_TIME = 1f;
        private IEnumerator comboDelayHandle = null;


        public override void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, IWeapon weap)
        {
            base.Initialize(player, fsm, cc, modelAnim, latencyInterpolationAnim, weap);

            attackMotionInfos = new();
            foreach (var type in Enum.GetValues(typeof(AttackMotionType)))
            {
                var scriptableMotionInfos = Resources.Load<PlayerAttackMotionInfoScriptable>($"{PlayerAttackMotionInfoScriptable.KEY_BASE}{type}").attackMotionInfos;

                attackMotionInfos.Add((AttackMotionType)type, scriptableMotionInfos);
            }
        }

        protected override void SetInfo(INetworkStruct info) => currentMotionInfo = ((StateInfo)info).attackInfo;

        protected override void EnterStateAuthority(int enterTick)
        {
            currentMotionIndex = currentCombo++ % attackMotionInfos[currentMotionInfo.attackMotionType].Count;
            currentMotion = ResolveAttackMotion();

            if (comboDelayHandle != null) StopCoroutine(comboDelayHandle);
            StartCoroutine(comboDelayHandle = ComboDelay(currentMotion.motionDuration + COMBO_DELAY_TIME));

            float tickRate = 1 / Runner.DeltaTime;
            attackEndTick = Runner.Tick + Mathf.RoundToInt(currentMotion.motionDuration * tickRate);
            attackRetryTick = attackEndTick - Mathf.RoundToInt(attackTryWindowTime * tickRate);

            currentMotionStartTick = Runner.Tick;
            currentAttackMove = Vector3.zero;

            var enemy = FindEnemy();
            if (enemy != null)
            {
                var dir = (enemy.transform.position - player.transform.position).normalized;
                SetLookRotation(Quaternion.LookRotation(dir));
            }
        }

        protected override void EnterStateShared(int enterTick)
        {
            currentMotion = currentMotion == null ? ResolveAttackMotion() : currentMotion;
            PlayAnim(currentMotion.motionName, .1f, enterTick);
        }

        private IEnumerator ComboDelay(float sec)
        {
            yield return new WaitForSeconds(sec);
            currentCombo = 0;
        }

        private AttackMotionInfo ResolveAttackMotion()
        {
            var targetList = attackMotionInfos[currentMotionInfo.attackMotionType];
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

            Move(currentAttackMove * attackMoveSpeed * Runner.DeltaTime);
        }

        protected override void ExitState()
        {
            weap.SetCollisionActive(false);

            var motionInfo = currentMotionInfo;
            motionInfo.attackMotionType = AttackMotionType.None;
            currentMotionInfo = motionInfo;
        }

        private Player FindEnemy()
        {
            return Player.RegistedUsers.FirstOrDefault(x => x.Key.Equals(Object.InputAuthority) == false).Value;
        }

        protected override void OnExitRender()
        {
            weap.SetTrailEffectActive(false);
            currentMotion = null;
        }

        protected override void OnMasterTick()
        {
            var currentMotion = ResolveAttackMotion();
            foreach (var timing in currentMotion.attackTimings)
            {
                int startTick = currentMotionStartTick + ConvertFrameToTick(timing.startTick, currentMotion.clip, Runner.TickRate);
                int endTick = currentMotionStartTick + ConvertFrameToTick(timing.endTick, currentMotion.clip, Runner.TickRate);

                bool active = Runner.Tick >= startTick && Runner.Tick <= endTick;
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
                            attackerPos = player.transform.position
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
                case "SetTrailEffect":
                    SetTrailEffectAcitve(parts[1]);
                    break;
                case "OnSlashEffect":
                    OnSlashEffect(parts[1]);
                    break;
            }
        }

        private Vector3 ConvertToVector3(string param)
        {
            string[] moveVector = param
                .Split(',')
                .Select(p => p.Trim())
                .ToArray();

            if (moveVector.Length == 3 &&
                float.TryParse(moveVector[0], out float x) &&
                float.TryParse(moveVector[1], out float y) &&
                float.TryParse(moveVector[2], out float z))
                return new Vector3(x, y, z);

            return Vector3.zero;
        }

        private void OnAttackMove(string param)
        {
            if (HasStateAuthority == false) return;

            if (string.IsNullOrEmpty(param) || param.Equals("0"))
            {
                currentAttackMove = Vector3.zero;
                return;
            }

            var input = ConvertToVector3(param);
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

        private void SetTrailEffectAcitve(string param)
        {
            weap.SetTrailEffectActive(param.Equals("0") == false);
        }

        private void OnSlashEffect(string param)
        {
            var parts = param.Split("/");

            Vector3 pos = ConvertToVector3(parts[0]);
            Vector3 rot = ConvertToVector3(parts[1]);

            weap.SetSlashEffectActive(pos, Quaternion.Euler(rot));
        }
    }
}