using UnityEngine;
using Fusion;

namespace Unit
{

    public class PlayerSprintState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Sprint;

        [SerializeField] private float sprintSpeed;

        private float curvSpeed = 10;
        private float animCurvSpeed = .1f;

        [Networked] public float runWeight { get; set; }
        private const float sprintRunWeight = 4;

        private MoveInfo currentMoveInfo;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = (MoveInfo)info;

        protected override void EnterState(bool sync = true)
        {
            PlayAnim("_Movement", .2f, sync);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (fsm.input.WasPressed(x => x.jump))
            {
                fsm.SetState<PlayerJumpState, MoveInfo>(currentMoveInfo, true);
                return;
            }

            if (fsm.input.Current.moveDir.sqrMagnitude < .01f || fsm.input.IsSet(x => x.dash) == false)
            {
                fsm.SetState<PlayerMovementState>();
                return;
            }

            if (fsm.input.IsSet(x => x.attack))
            {
                fsm.SetState<PlayerAttackState, AttackInfo>(new() { attackMotionType = AttackMotionType.Dash });
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

            cc.SetLookRotation(Quaternion.Slerp(cc.transform.rotation, Quaternion.LookRotation(inputDir), curvSpeed * Runner.DeltaTime));
            cc.Move(currentMoveInfo.moveDir * currentMoveInfo.velocity * Runner.DeltaTime);
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