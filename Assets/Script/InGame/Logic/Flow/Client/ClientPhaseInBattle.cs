using InGame.Logic.Flow;
using System.Threading.Tasks;
using Unit;
using Utility.Spinner;

namespace InGame.Logic
{
    public class ClientPhaseInBattle : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.InBattle;

        public override async Task OnEnter()
        {
            Spinner.Instance.OffSpinner();

            if (Player.RegistedUsers.TryGetValue(runner.LocalPlayer, out var player))
            {
                player.SetCanController(true);
            }

            const int CHECK_DELAY_MS = 500;
            bool allUserAlive = true;
            while (allUserAlive)
            {
                foreach (var user in Player.RegistedUsers.Values)
                {
                    if (user.isAlive() == false)
                    {
                        allUserAlive = false;
                        break;
                    }
                }

                await Task.Delay(CHECK_DELAY_MS);
            }

            phaseDoneListener?.Invoke();
        }
    }
}