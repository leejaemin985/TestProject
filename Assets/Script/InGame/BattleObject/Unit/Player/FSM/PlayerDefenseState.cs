using UnityEngine;
using Fusion;

namespace Unit
{
    public class PlayerDefenseState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Defense;

        protected override StatePriorityType Priority => StatePriorityType.Free;

        public const float defenseStartupTime = 0.1f;

        [SerializeField] private float defenseMoveSpeed;
        [SerializeField] private float curvSpeed = 10;

        private Vector3 currentMoveDir;


        [Networked] private Vector3 moveAnimDir { get; set; }

        protected override void EnterStateShared(int enterTick)
        {
            PlayAnim("_DefenseMove", .15f, enterTick);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            if (fsm.input.IsSet(x => x.defense == false))
            {
                fsm.SetState<PlayerMovementState>();
                return;
            }

            Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);
            moveAnimDir = Vector3.Lerp(moveAnimDir, inputDir, curvSpeed * Runner.DeltaTime);

            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();

            currentMoveDir = Vector3.Lerp(currentMoveDir, inputDir, curvSpeed * Runner.DeltaTime);

            if (inputDir.sqrMagnitude > .01f)
            {
                SetLookRotation(Quaternion.Slerp(player.transform.rotation, Camera.main.transform.rotation, curvSpeed * Runner.DeltaTime));
            }
            Move(currentMoveDir * defenseMoveSpeed * Runner.DeltaTime);
        }

        protected override void OnRender()
        {
            const string HORIZONTAL = "_Horizontal";
            const string VERTICAL = "_Vertical";

            float curvSpeed = .1f;

            float currentHorizontal = modelAnim.GetFloat(HORIZONTAL);
            float currentVertical = modelAnim.GetFloat(VERTICAL);

            modelAnim.SetFloat(HORIZONTAL, Mathf.Lerp(currentHorizontal, moveAnimDir.x, curvSpeed));
            modelAnim.SetFloat(VERTICAL, Mathf.Lerp(currentVertical, moveAnimDir.z, curvSpeed));
        }
    }
}