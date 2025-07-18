using UnityEngine;

namespace Unit
{
    public class PlayerMovementState : PlayerStateBase
    {
        public override StateType stateType => StateType.Move;

        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;

        [SerializeField] private float curveSpeed = 10;

        private Vector3 currentMoveDir;
        private float currentMoveSpeed;

        protected override void EnterState()
        {
            base.EnterState();

            currentMoveDir = Vector3.zero;
            currentMoveSpeed = walkSpeed;

            fsm.RPC_RunMotion(animState, fsm.cachedTick, .3f);
        }

        protected override void OnState()
        {
            if (!fsm.HasAuthority) return;

            if (fsm.input.IsSet(x => x.attack))
            {
                fsm.SetState<PlayerAttackState>();
                return;
            }

            Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);

            fsm.player.moveAnimDir = Vector3.Lerp(fsm.player.moveAnimDir, inputDir, curveSpeed * fsm.deltaTime);

            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();

            currentMoveDir = Vector3.Lerp(currentMoveDir, inputDir, curveSpeed * fsm.deltaTime);

            float targetMoveSpeed = fsm.input.IsSet(x => x.dash) ? runSpeed : walkSpeed;
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, curveSpeed * fsm.deltaTime);

            fsm.player.runWeight = Mathf.Lerp(1f, 2f, Mathf.InverseLerp(walkSpeed, runSpeed, currentMoveSpeed));

            if (inputDir.sqrMagnitude > 0.01f)
            {
                fsm.cc.SetLookRotation(Quaternion.Slerp(fsm.cc.transform.rotation, Camera.main.transform.rotation, curveSpeed * fsm.deltaTime));
            }

            fsm.cc.Move(inputDir * currentMoveSpeed * fsm.deltaTime);
        }

        protected override void OnRender()
        {
            const string HORIZONTAL = "_Horizontal";
            const string VERTICAL = "_Vertical";
            const string RUNWEIGHT = "_RunWeight";

            float curvSpeed = .1f;

            float currentHorizontal = fsm.anim.GetFloat(HORIZONTAL);
            float currentVertical = fsm.anim.GetFloat(VERTICAL);
            float currentRunWeight = fsm.anim.GetFloat(RUNWEIGHT);

            fsm.anim.SetFloat(HORIZONTAL, Mathf.Lerp(currentHorizontal, fsm.player.moveAnimDir.x, curvSpeed));
            fsm.anim.SetFloat(VERTICAL, Mathf.Lerp(currentVertical, fsm.player.moveAnimDir.z, curvSpeed));
            fsm.anim.SetFloat(RUNWEIGHT, Mathf.Lerp(currentRunWeight, fsm.player.runWeight, curvSpeed));
        }
    }
}