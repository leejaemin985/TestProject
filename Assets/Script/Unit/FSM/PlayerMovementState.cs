using UnityEngine;
using Fusion;

namespace Unit
{
    public class PlayerMovementState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Move;

        [SerializeField] private float walkSpeed;
        [SerializeField] private float curveSpeed = 10;

        private MoveInfo currentMoveInfo;

        [Networked] private Vector3 moveAnimDir { get; set; }
        [Networked] private float runWeight { get; set; }

        private const float walkRunWeight = 1;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = (MoveInfo)info;

        protected override void EnterState(bool sync = true)
        {
            base.EnterState();

            PlayAnim("_Movement", .2f, sync);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (fsm.input.Current.moveDir.sqrMagnitude > .01f && fsm.input.IsSet(x => x.dash))
            {
                fsm.SetState<PlayerSprintState, MoveInfo>(currentMoveInfo, true);
                return;
            }

            if (fsm.input.WasPressed(x => x.jump))
            {
                fsm.SetState<PlayerJumpState, MoveInfo>(currentMoveInfo, true);
                return;
            }

            if (fsm.input.IsSet(x => x.attack))
            {
                fsm.SetState<PlayerAttackState, AttackInfo>(new() { attackMotionType = AttackMotionType.None });
                return;
            }

            if (fsm.input.IsSet(x => x.defense))
            {
                fsm.SetState<PlayerDefenseState>();
                return;
            }

            Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);

            moveAnimDir = Vector3.Lerp(moveAnimDir, inputDir, curveSpeed * Runner.DeltaTime);

            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();

            currentMoveInfo.moveDir = Vector3.Lerp(currentMoveInfo.moveDir, inputDir, curveSpeed * Runner.DeltaTime);

            float targetMoveSpeed = walkSpeed;
            currentMoveInfo.velocity = Mathf.Lerp(currentMoveInfo.velocity, targetMoveSpeed, curveSpeed * Runner.DeltaTime);

            float speedRatio = currentMoveInfo.velocity / walkSpeed;
            runWeight = speedRatio * walkRunWeight;

            if (inputDir.sqrMagnitude > 0.01f)
            {
                cc.SetLookRotation(Quaternion.Slerp(cc.transform.rotation, Camera.main.transform.rotation, curveSpeed * Runner.DeltaTime));
            }

            cc.Move(inputDir * currentMoveInfo.velocity * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            const string HORIZONTAL = "_Horizontal";
            const string VERTICAL = "_Vertical";
            const string RUNWEIGHT = "_RunWeight";

            float curvSpeed = .2f;

            float currentHorizontal = anim.GetFloat(HORIZONTAL);
            float currentVertical = anim.GetFloat(VERTICAL);
            float currentRunWeight = anim.GetFloat(RUNWEIGHT);

            anim.SetFloat(HORIZONTAL, Mathf.Lerp(currentHorizontal, moveAnimDir.x, curvSpeed));
            anim.SetFloat(VERTICAL, Mathf.Lerp(currentVertical, moveAnimDir.z, curvSpeed));
            anim.SetFloat(RUNWEIGHT, Mathf.Lerp(currentRunWeight, runWeight, curvSpeed));
        }
    }
}