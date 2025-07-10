using UnityEngine;

namespace Unit
{
    public class PlayerMovementState : PlayerStateBase
    {
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;

        [SerializeField] private float curveSpeed = 10;

        private Vector3 currentMoveDir;
        private float currentMoveSpeed;

        protected override bool CanEnter()
        {
            return true;
        }

        protected override void EnterState()
        {
            currentMoveDir = Vector3.zero;
            currentMoveSpeed = walkSpeed;
            fsm.anim.Play(animState);
        }

        protected override void OnState()
        {
            if (fsm.HasAuthority)
            {
                Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);

                //fsm.moveAnimDir = Vector3.Lerp(fsm.moveAnimDir, inputDir, curveSpeed * fsm.deltaTime);
                //fsm.moveAnimDir = inputDir;//Vector3.Lerp(fsm.moveAnimDir, inputDir, curveSpeed * fsm.deltaTime);
                fsm.player.moveAnimDir = inputDir;


                inputDir = Camera.main.transform.TransformDirection(inputDir);
                inputDir.y = 0;
                inputDir.Normalize();

                //currentMoveDir = Vector3.Lerp(currentMoveDir, inputDir, curveSpeed * fsm.deltaTime);

                float targetMoveSpeed = fsm.input.IsSet(x => x.dash) ? runSpeed : walkSpeed;
                //currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, curveSpeed * fsm.deltaTime);
                
                //fsm.runWeight = Mathf.Lerp(1f, 2f, Mathf.InverseLerp(walkSpeed, runSpeed, targetMoveSpeed));
                fsm.player.runWeight = Mathf.Lerp(1f, 2f, Mathf.InverseLerp(walkSpeed, runSpeed, targetMoveSpeed));

                if (inputDir.sqrMagnitude > 0.01f)
                {
                    fsm.cc.SetLookRotation(Quaternion.Slerp(fsm.cc.transform.rotation, Camera.main.transform.rotation, curveSpeed * fsm.deltaTime));
                }

                fsm.cc.Move(inputDir * currentMoveSpeed * fsm.deltaTime);
            }

            //fsm.anim.SetFloat("_Horizontal", fsm.moveAnimDir.x);
            //fsm.anim.SetFloat("_Vertical", fsm.moveAnimDir.z);
            //fsm.anim.SetFloat("_RunWeight", fsm.runWeight);
        }
    }
}