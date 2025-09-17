using CustomPhysics;
using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{
    public class PlayerRoarState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Roar;
        protected override StatePriorityType Priority => StatePriorityType.Override;

        [SerializeField] private AttackBox physicsRange;

        [SerializeField] private float damage;
        [SerializeField] private float weight;
        [SerializeField] private AttackType attackType;

        private const float roarMotionDuration = .4f;

        [Networked] private int roarStartTick { get; set; }
        [Networked] private int roarEndTick { get; set; }

        public override void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, IWeapon weap)
        {
            base.Initialize(player, fsm, cc, modelAnim, latencyInterpolationAnim, weap);

            physicsRange.Initialize(OnHit);
            physicsRange.AddIgnoreUid(player.playerHitBox);
        }

        protected override void EnterState(PlayerFSM.TransitionType transitionType, bool sync = true)
        {
            roarEndTick = Runner.Tick + Mathf.RoundToInt(roarMotionDuration * Runner.TickRate);
            PlayAnim(transitionType, Priority, "Roar", .1f, true);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (Runner.Tick > roarEndTick)
            {
                fsm.SetState<PlayerMovementState>(PlayerFSM.TransitionType.System);
            }
        }

        //TestCode######################################################################################
        protected override void OnExitRender()
        {
            physicsRange.SetActive(false);
        }

        protected override void OnMasterTick()
        {
            bool inRange = Runner.Tick > roarStartTick && Runner.Tick < roarEndTick;
            if (inRange)
            {
                if (!physicsRange.Active) physicsRange.SetActive(true);
            }
            else
            {
                physicsRange.SetActive(false);
            }
        }

        private void OnHit(CollisionInfos collisionInfos)
        {
            var hitInfo = new HitInfo()
            {
                damaged = damage,
                weight = weight,
                attackType = attackType,
                attackerPos = player.transform.position
            };

            foreach (var info in collisionInfos.collisionInfos)
            {
                info.hitObject.OnHitEvent(new PlayerCollisionInfo(info, hitInfo));
            }
        }
    }
}
