using InGame.Logic.Flow;
using System.Threading.Tasks;
using Unit;
using Utility.Spinner;

namespace InGame.Logic
{
    public class ClientPhaseInBattle : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.InBattle;

        protected override Task<PhaseReport> OnEnter(PhaseDirective phaseDirective)
        {
            Spinner.Instance.OffSpinner();

            if (Player.RegistedUsers.TryGetValue(runner.LocalPlayer, out var player))
                player.SetCanController(true);

            return Task.FromResult(CreatePhaseReport(PhaseState.Init));
        }

        protected override PhaseState OnTick(float deltaTime)
        {
            bool aliveAllUser = true;
            foreach (var user in Player.RegistedUsers.Values)
            {
                if (user.isAlive() == false)
                {
                    aliveAllUser = false;
                    break;
                }
            }

            return aliveAllUser ? PhaseState.Run : PhaseState.Wait;
        }

    }
}