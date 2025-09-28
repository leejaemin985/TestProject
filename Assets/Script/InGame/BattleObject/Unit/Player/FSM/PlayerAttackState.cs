using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.Networking;
using InGame.Event;

namespace Unit
{
    public class PlayerAttackState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Attack;

        protected override StatePriorityType Priority => StatePriorityType.Free;


        private Dictionary<AttackMotionType, List<AttackMotionInfo>> attackMotionInfos;

        [Networked] private int currentMotionStartTick { get; set; }
        [Networked] private int currentMotionIndex { get; set; }
        private AttackInfo currentMotionInfo { get; set; }
        private AttackMotionInfo currentMotion;

        private const float ATTACK_RETRY_WINDOW_TIME = .1f;
        private int attackEndTick;
        private int attackRetryTick;
        private int currentCombo;

        private const float ATTACK_MOVE_DISTANCE_OFFSET = .5f;
        private const float ATTACK_MOVE_RATIO_CLAMP_MAX = 2f;
        private Vector3 currentAttackMove;
        private float attackMoveSpeed = 100f;

        private const float COMBO_DELAY_TIME = 1f;
        private IEnumerator comboDelayHandle = null;

        private Player Enemy => Player.RegistedUsers.FirstOrDefault(x => x.Key.Equals(Object.InputAuthority) == false).Value;

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

        #region FSM State
        //EnterState
        protected override void EnterStateAuthority(int enterTick)
        {
            //모션 인포 설정
            currentMotionIndex = currentCombo++ % attackMotionInfos[currentMotionInfo.attackMotionType].Count;
            currentMotion = ResolveAttackMotion();
            if (comboDelayHandle != null) StopCoroutine(comboDelayHandle);
            StartCoroutine(comboDelayHandle = ComboDelay(currentMotion.motionDuration + COMBO_DELAY_TIME));

            //상태 유지시간 설정
            float tickRate = 1 / Runner.DeltaTime;
            attackEndTick = Runner.Tick + Mathf.RoundToInt(currentMotion.motionDuration * tickRate);
            attackRetryTick = attackEndTick - Mathf.RoundToInt(ATTACK_RETRY_WINDOW_TIME * tickRate);

            currentMotionStartTick = Runner.Tick;
            currentAttackMove = Vector3.zero;

            var enemy = Enemy;
            if (enemy != null)
            {
                var dir = (enemy.transform.position - player.transform.position).normalized;
                SetLookRotation(Quaternion.LookRotation(dir));
            }
        }

        protected override void EnterStateShared(int enterTick)
        {
            currentMotion = HasStateAuthority == false ? ResolveAttackMotion() : currentMotion;
            PlayAnim(currentMotion.motionName, .1f, enterTick);
        }
        

        //ExitState
        protected override void ExitStateShared()
        {
            weap.SetCollisionActive(false);

            var motionInfo = currentMotionInfo;
            motionInfo.attackMotionType = AttackMotionType.None;
            currentMotionInfo = motionInfo;

            weap.SetTrailEffectActive(false);
            currentMotion = null;
        }


        //OnState
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

        //AnimEvent
        protected override void OnAnimEvent(AnimationEventData data)
        {
            switch (data)
            {
                case PlayerMoveAnimEventData moveData:
                    OnAttackMove(moveData);
                    break;

                case WeapTrailAnimEventData trailData:
                    SetTrailEffectAcitve(trailData);
                    break;

                case WeapCollisionAnimEvent collisionData:
                    break;

                case WeapSlashEffectEventData slashEffectData:
                    OnSlashEffect(slashEffectData);
                    break;
            }
        }
        #endregion


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


        private int ConvertFrameToTick(int frame, AnimationClip clip, float tickRate)
        {
            float sec = frame / clip.frameRate;
            return Mathf.RoundToInt(sec * tickRate);
        }

        private void OnAttackMove(PlayerMoveAnimEventData data)
        {
            if (HasStateAuthority == false) return;

            if (data.MoveDir == Vector3.zero)
            {
                currentAttackMove = Vector3.zero;
                return;
            }

            Vector3 forward = player.transform.forward;
            float moveRatio = 1f;

            var enemy = Enemy;
            if (enemy != null)
            {
                Vector3 toEnemy = (enemy.transform.position - player.transform.position);
                toEnemy.y = 0;
                toEnemy.Normalize();

                forward = toEnemy.normalized;
                moveRatio = Mathf.Clamp((toEnemy.magnitude - ATTACK_MOVE_DISTANCE_OFFSET) / 1f, 0, ATTACK_MOVE_RATIO_CLAMP_MAX);
            }
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            Vector3 rawMove = (forward * data.MoveDir.z + right * data.MoveDir.x).normalized;
            Vector3 worldMoveDir = rawMove * data.MoveDir.magnitude;

            currentAttackMove = worldMoveDir * moveRatio;
            attackMoveSpeed = data.MoveSpeed;
        }

        private void SetTrailEffectAcitve(WeapTrailAnimEventData trailData)
        {
            weap.SetTrailEffectActive(trailData.SetTrail);
        }

        private void OnSlashEffect(WeapSlashEffectEventData slashEffectData)
        {
            weap.SetSlashEffectActive(slashEffectData.LocalPos, slashEffectData.LocalRot);
        }
    }
}