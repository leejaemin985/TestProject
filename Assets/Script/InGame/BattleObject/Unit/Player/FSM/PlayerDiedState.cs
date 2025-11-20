using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using Utility.EffectObject;
using Utility.Sound;

namespace Unit
{
    public class PlayerDiedState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Died;

        protected override StatePriorityType Priority => StatePriorityType.Terminal;

        private HitInfo currentHitInfo;

        private string[] dieMotionNames = new string[]
        {
            "Died_1",
            "Died_2",
            "Died_3"
        };

        private EffectObjectPool deathBloodEffectPool;
        //private AudioClip

        protected override void SetInfo(INetworkStruct info) => currentHitInfo = ((StateInfo)info).hitInfo;

        #region FSM State
        public override void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, ISoundObject soundObject, IWeapon weap)
        {
            base.Initialize(player, fsm, cc, modelAnim, latencyInterpolationAnim, soundObject, weap);
            LoadEffect();
        }

        //EnterState
        protected override void EnterStateShared(int enterTick)
        {
            PlayAnim(dieMotionNames[UnityEngine.Random.Range(0, dieMotionNames.Length)], .1f, enterTick);
            OnDeathBloodEffect();
        }
        #endregion

        private void LoadEffect()
        {
            deathBloodEffectPool =
                EffectObjectPool.CreatePoolInstance<DeathStateBloodEffect>(
                    (DeathStateBloodEffect)InGamePlayerResourcesLoader.userStateEffectAsset.deathStateBloodEffect,
                    new() { count = 1, effectRoot = null });
        }

        private void OnDeathBloodEffect()
        {
            deathBloodEffectPool.OnPlayEffect(
                player.transform.position + new Vector3(0, 1.2f, 0),
                player.transform.rotation);
        }
    }
}