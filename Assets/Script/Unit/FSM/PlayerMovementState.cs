using UnityEngine;
using Fusion;

namespace Unit
{
    public class PlayerMovementState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Move;

        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;

        [SerializeField] private float curveSpeed = 10;

        private Vector3 currentMoveDir;
        private float currentMoveSpeed;


        [Networked] private Vector3 moveAnimDir { get; set; }
        [Networked] private float runWeight { get; set; }

        protected override void EnterState(bool sync = true)
        {
            base.EnterState();
            
            currentMoveDir = Vector3.zero;
            currentMoveSpeed = walkSpeed;

            PlayAnim("_Movement", .2f, sync);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (fsm.input.IsSet(x => x.attack))
            {
                fsm.SetState<PlayerAttackState>();
                return;
            }
            else if (fsm.input.IsSet(x => x.defense))
            {
                fsm.SetState<PlayerDefenseState>();
                return;
            }

            Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);

            moveAnimDir = Vector3.Lerp(moveAnimDir, inputDir, curveSpeed * Runner.DeltaTime);

            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();

            currentMoveDir = Vector3.Lerp(currentMoveDir, inputDir, curveSpeed * Runner.DeltaTime);

            float targetMoveSpeed = fsm.input.IsSet(x => x.dash) ? runSpeed : walkSpeed;
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, curveSpeed * Runner.DeltaTime);

            runWeight = Mathf.Lerp(1f, 2f, Mathf.InverseLerp(walkSpeed, runSpeed, currentMoveSpeed));

            if (inputDir.sqrMagnitude > 0.01f)
            {
                cc.SetLookRotation(Quaternion.Slerp(cc.transform.rotation, Camera.main.transform.rotation, curveSpeed * Runner.DeltaTime));
            }

            cc.Move(inputDir * currentMoveSpeed * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            const string HORIZONTAL = "_Horizontal";
            const string VERTICAL = "_Vertical";
            const string RUNWEIGHT = "_RunWeight";

            float curvSpeed = .1f;

            float currentHorizontal = anim.GetFloat(HORIZONTAL);
            float currentVertical = anim.GetFloat(VERTICAL);
            float currentRunWeight = anim.GetFloat(RUNWEIGHT);

            anim.SetFloat(HORIZONTAL, Mathf.Lerp(currentHorizontal, moveAnimDir.x, curvSpeed));
            anim.SetFloat(VERTICAL, Mathf.Lerp(currentVertical, moveAnimDir.z, curvSpeed));
            anim.SetFloat(RUNWEIGHT, Mathf.Lerp(currentRunWeight, runWeight, curvSpeed));
        }
    }
}