using System.Threading.Tasks;
using Fusion;
using InGame.Logic.Flow;
using Unit;

namespace InGame.Logic
{
    public class ClientPhaseEnd : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.End;

        protected override Task<PhaseState> OnEnter(PhaseDirective phaseDirective)
        {
            if (Player.RegistedUsers.TryGetValue(runner.LocalPlayer, out var user))
                runner.Despawn(user.Object);

            return Task.FromResult(PhaseState.Wait);
        }
    }
}