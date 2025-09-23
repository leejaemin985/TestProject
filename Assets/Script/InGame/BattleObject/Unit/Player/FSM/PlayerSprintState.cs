using UnityEngine;
using Fusion;

namespace Unit
{

    public class PlayerSprintState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Sprint;

        protected override StatePriorityType Priority => StatePriorityType.Free;

        [SerializeField] private float sprintSpeed;

        private float curvSpeed = 10;
        private float animCurvSpeed = .1f;

        [Networked] public float runWeight { get; set; }
        private const float sprintRunWeight = 4;

        private MoveInfo currentMoveInfo;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = ((StateInfo)info).moveInfo;

        protected override void EnterState(int enterTick)
        {
            PlayAnim("_Movement", .2f, enterTick);
        }

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

            currentMoveInfo.moveDir = Vector3.Lerp(currentMoveInfo.moveDir, inputDir, curvSpeed * Runner.DeltaTime);
            currentMoveInfo.velocity = Mathf.Lerp(currentMoveInfo.velocity, sprintSpeed, curvSpeed * Runner.DeltaTime);

            float speedRatio = currentMoveInfo.velocity / sprintSpeed;
            runWeight = speedRatio * sprintRunWeight;

            SetLookRotation(Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(inputDir), curvSpeed * Runner.DeltaTime));
            Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            const string HORIZONTAL = "_Horizontal";
            const string VERTICAL = "_Vertical";
            const string RUNWEIGHT = "_RunWeight";
            
            float currentHorizontal = modelAnim.GetFloat(HORIZONTAL);
            float currentVertical = modelAnim.GetFloat(VERTICAL);
            float currentRunWeight = modelAnim.GetFloat(RUNWEIGHT);

            modelAnim.SetFloat(HORIZONTAL, Mathf.Lerp(currentHorizontal, 0, animCurvSpeed));
            modelAnim.SetFloat(VERTICAL, Mathf.Lerp(currentVertical, 1, animCurvSpeed));
            modelAnim.SetFloat(RUNWEIGHT, Mathf.Lerp(currentRunWeight, runWeight, animCurvSpeed));
        }
    }
}