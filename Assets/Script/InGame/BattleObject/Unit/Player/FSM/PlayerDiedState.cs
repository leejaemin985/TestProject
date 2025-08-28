namespace Unit
{
    public class PlayerDiedState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Died;

        private string[] dieMotionNames = new string[]
        {
            "Died_1",
            "Died_2",
            "Died_3"
        };

        protected override void EnterState(bool sync = true)
        {
            PlayAnim(dieMotionNames[UnityEngine.Random.Range(0, dieMotionNames.Length)], .1f, false);
        }
    }
}