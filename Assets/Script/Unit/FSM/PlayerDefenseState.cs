using Fusion;
using UnityEngine;

namespace Unit
{
    public class PlayerDefenseState : PlayerStateBase
    {
        public override StateType GetStateType() => StateType.Defense;
        public const float defenseStartupTime = 0.1f;

        protected override void EnterState(bool sync = true)
        {
            PlayAnim("_Defense", .15f, true);
        }

        protected override void OnState()
        {
            if (!HasStateAuthority) return;
            if (fsm.input.IsSet(x => x.defense == false))
            {
                fsm.SetState<PlayerMovementState>();
                return;
            }
        }
    }
}