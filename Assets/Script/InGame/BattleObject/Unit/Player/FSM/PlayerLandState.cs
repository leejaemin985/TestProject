using UnityEngine;
using Fusion.Addons.SimpleKCC;

using Utility.Sound;
using InGame.Event;

namespace Unit
{

    public class PlayerLandState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Land;

        protected override StatePriorityType Priority => StatePriorityType.Free;

        private const float MIN_DURATION = .1f;
        private const float DEFAULT_DURATION = .3f;
        
        private int minLandingMotionEndTick;
        private int landingMotionEndTick;

        private AudioClip[] landingSE;

        #region FSM State
        public override void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, ISoundObject soundObject, IWeapon weap)
        {
            base.Initialize(player, fsm, cc, modelAnim, latencyInterpolationAnim, soundObject, weap);

            landingSE = InGamePlayerResourcesLoader.soundPack.landStateSE;
        }

        //EnterState
        protected override void EnterStateAuthority(int enterTick)
        {
            minLandingMotionEndTick = Runner.Tick + Mathf.RoundToInt(MIN_DURATION * Runner.TickRate);
            landingMotionEndTick = Runner.Tick + Mathf.RoundToInt(DEFAULT_DURATION * Runner.TickRate);
        }

        protected override void EnterStateShared(int enterTick)
        {
            PlayAnim(true ? "_LandingWait" : "_LandingMove", .1f, enterTick);
        }

        //OnState
        protected override void OnState()
        {
            if (!HasInputAuthority) return;

            bool canExitState = !fsm.input.Current.IsInputEmpty() && Runner.Tick > minLandingMotionEndTick;
            bool doneState = Runner.Tick > landingMotionEndTick;

            if (canExitState || doneState)
            {
                fsm.SetState<PlayerMovementState>();
            }
        }

        protected override void OnAnimEvent(AnimationEventData data)
        {
            switch (data)
            {
                case LandingSoundEffectAnimEventData landingSEAnimData:
                    PlayLandingSE(landingSEAnimData);
                    break;
            }
        }
        #endregion

        private void PlayLandingSE(LandingSoundEffectAnimEventData landingSEAnimData)
        {
            soundObject.PlayOneShot(landingSE[Random.Range(0, landingSE.Length)]);
        }
    }
}