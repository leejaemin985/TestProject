namespace Unit
{
    using Fusion;
    using UnityEngine;

    public class PlayerSprintState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Sprint;

        [SerializeField] private float sprintSpeed;

        [Networked] public float runWeight { get; set; }

        private MoveInfo currentMoveInfo;

        protected override void SetInfo(INetworkStruct info) => currentMoveInfo = (MoveInfo)info;

        protected override void EnterState(bool sync = true)
        {
            PlayAnim("_Sprint", 0.1f, true);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;

            Vector3 inputDir = new Vector3(fsm.input.Current.moveDir.x, 0, fsm.input.Current.moveDir.y);
            runWeight

        }

        protected override void OnRender()
        {
            anim.SetFloat("_RunWeight", runWeight);
        }
    }
}