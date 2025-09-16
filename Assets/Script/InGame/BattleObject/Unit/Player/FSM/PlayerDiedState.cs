namespace Unit
{
    public class PlayerDiedState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Died;

        protected override StatePriorityType Priority => StatePriorityType.Terminal;

        private string[] dieMotionNames = new string[]
        {
            "Died_1",
            "Died_2",
            "Died_3"
        };

        protected override void EnterState(PlayerFSM.TransitionType transitionType, bool sync = true)
        {
            PlayAnim(transitionType, Priority, dieMotionNames[UnityEngine.Random.Range(0, dieMotionNames.Length)], .1f, false);
        }
    }
}