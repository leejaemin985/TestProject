using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Utility.Sound;
using InGame.Event;

namespace Unit
{

    public class PlayerSprintState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Sprint;

        protected override StatePriorityType Priority => StatePriorityType.Free;

        [SerializeField] private float sprintSpeed;

        private const float CURV_SPEED = 10;

        [Networked] public float runWeight { get; set; }
        private const float SPRINT_RUNWEIGHT = 4;

        private MoveInfo currentMoveInfo;

        private AudioClip[] sprintStepSE;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = ((StateInfo)info).moveInfo;

        #region FSM State

        public override void Initialize(Player player, PlayerFSM fsm, SimpleKCC cc, Animator modelAnim, Animator latencyInterpolationAnim, ISoundObject soundObject, IWeapon weap)
        {
            base.Initialize(player, fsm, cc, modelAnim, latencyInterpolationAnim, soundObject, weap);

            sprintStepSE = InGamePlayerResourcesLoader.soundPack.sprintStateSE;
        }

        //EnterState
        protected override void EnterStateShared(int enterTick)
        {
            PlayAnim("_Movement", .2f, enterTick);
        }

        //OnState
        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (fsm.input.WasPressed(x => x.jump))
            {
                fsm.SetState<PlayerJumpState>(new StateInfo()
                {
                    moveInfo = currentMoveInfo
                });
                return;
            }

            if (fsm.input.Current.moveDir.sqrMagnitude < .01f || fsm.input.IsSet(x => x.dash) == false)
            {
                fsm.SetState<PlayerMovementState>();
                return;
            }

            if (fsm.input.IsSet(x => x.attack))
            {
                fsm.SetState<PlayerAttackState>(
                    new StateInfo()
                    {
                        attackInfo = new() { attackMotionType = AttackMotionType.Dash }
                    });
                return;
            }

            Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);
            
            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();

            currentMoveInfo.moveDir = Vector3.Lerp(currentMoveInfo.moveDir, inputDir, CURV_SPEED * Runner.DeltaTime);
            currentMoveInfo.velocity = Mathf.Lerp(currentMoveInfo.velocity, sprintSpeed, CURV_SPEED * Runner.DeltaTime);

            float speedRatio = currentMoveInfo.velocity / sprintSpeed;
            runWeight = speedRatio * SPRINT_RUNWEIGHT;

            SetLookRotation(Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(inputDir), CURV_SPEED * Runner.DeltaTime));
            Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            const string HORIZONTAL = "_Horizontal";
            const string VERTICAL = "_Vertical";
            const string RUNWEIGHT = "_RunWeight";
            const float ANIM_CURV_SPEED = .1f;
            
            float currentHorizontal = modelAnim.GetFloat(HORIZONTAL);
            float currentVertical = modelAnim.GetFloat(VERTICAL);
            float currentRunWeight = modelAnim.GetFloat(RUNWEIGHT);

            modelAnim.SetFloat(HORIZONTAL, Mathf.Lerp(currentHorizontal, 0, ANIM_CURV_SPEED));
            modelAnim.SetFloat(VERTICAL, Mathf.Lerp(currentVertical, 1, ANIM_CURV_SPEED));
            modelAnim.SetFloat(RUNWEIGHT, Mathf.Lerp(currentRunWeight, runWeight, ANIM_CURV_SPEED));
        }

        protected override void OnAnimEvent(AnimationEventData data)
        {
            switch (data)
            {
                case SprintStepSEAnimEvent sprintStepSEEventData:
                    SprintStepSEAnimEvent(sprintStepSEEventData);
                    break;
            }
        }
        #endregion

        private void SprintStepSEAnimEvent(SprintStepSEAnimEvent stepSEData)
        {
            soundObject.PlayOneShot(sprintStepSE[Random.Range(0, sprintStepSE.Length)]);
        }
    }
}