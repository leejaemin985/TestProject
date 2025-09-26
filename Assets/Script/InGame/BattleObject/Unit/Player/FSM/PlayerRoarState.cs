using Addressable;
using CustomPhysics;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.EffectObject;

namespace Unit
{
    public class PlayerRoarState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Roar;
        protected override StatePriorityType Priority => StatePriorityType.Override;

        public override bool HasSuperArmor => true;

        [SerializeField] private AttackBox physicsRange;

        [SerializeField] private float damage;
        [SerializeField] private float weight;
        [SerializeField] private AttackType attackType;

        private const float roarMotionDuration = .8f;

        [Networked] private int roarStartTick { get; set; }
        [Networked] private int roarEndTick { get; set; }

        private EffectObjectPool effectPool;

        public override void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, IWeapon weap)
        {
            base.Initialize(player, fsm, cc, modelAnim, latencyInterpolationAnim, weap);

            LoadEffect();
            physicsRange.Initialize(OnHit);
            physicsRange.AddIgnoreUid(player.playerHitBox);
        }

        private async void LoadEffect()
        {
            var ob = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_UserStateEffectGroup);
            var effectGroup = ob.GetComponent<AddressableAsset_UserStateEffect>();
            effectPool = EffectObjectPool.CreatePoolInstance<RoarStateEffect>((RoarStateEffect)effectGroup.roarStateEffect, new() { count = 2, effectRoot = null });
        }

        #region FSM State
        //EnterState
        protected override void EnterStateAuthority(int enterTick)
        {
            roarEndTick = Runner.Tick + Mathf.RoundToInt(roarMotionDuration * Runner.TickRate);
        }

        protected override void EnterStateShared(int enterTick)
        {
            player.UnitStat.OnSuperArmor(roarEndTick);
            PlayAnim("Roar", .1f, enterTick);
        }

        //ExitState
        protected override void ExitStateShared()
        {
            physicsRange.SetActive(false);
        }
        
        //OnState
        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (Runner.Tick > roarEndTick)
            {
                fsm.SetState<PlayerMovementState>(default, TransitionType.System);
            }
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

        //AnimEvent
        protected override void OnAnimEvent(string param)
        {
            var parts = param.Split("//");
            switch (parts[0])
            {
                case "OnRoarEffect":
                    OnRoarEffect();
                    break;
            }
        }
        #endregion

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

        private void OnRoarEffect()
        {
            effectPool.OnPlayEffect(player.transform.position, Quaternion.identity);
        }
    }
}
