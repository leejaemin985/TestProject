using UnityEngine;

namespace Unit
{
    public class PlayerMovementState : PlayerStateBase
    {
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;

        protected override bool CanEnter()
        {
            return true;
        }

        protected override void EnterState()
        {
            fsm.anim.Play(animState);
        }

        protected override void OnState()
        {
            if (fsm.HasInputAuthority)
            {
                Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);
                
                fsm.moveAnimDir = inputDir;

                inputDir = Camera.main.transform.TransformDirection(inputDir);
                inputDir.y = 0;
                inputDir.Normalize();

                float moveSpeed = fsm.input.IsSet(x => x.dash) ? runSpeed : walkSpeed;
                fsm.runWeight = Mathf.Lerp(1f, 2f, Mathf.InverseLerp(walkSpeed, runSpeed, moveSpeed));

                if (inputDir.sqrMagnitude > 0.01f)
                {
                    fsm.cc.SetLookRotation(Quaternion.Slerp(fsm.cc.transform.rotation, Camera.main.transform.rotation, 10 * fsm.Runner.DeltaTime));
                }

                fsm.cc.Move(inputDir * moveSpeed * fsm.Runner.DeltaTime);
            }

            fsm.anim.SetFloat("_Horizontal", fsm.moveAnimDir.x);
            fsm.anim.SetFloat("_Horizontal", fsm.moveAnimDir.z);
            fsm.anim.SetFloat("_RunWeight", fsm.runWeight);
        }
    }
}