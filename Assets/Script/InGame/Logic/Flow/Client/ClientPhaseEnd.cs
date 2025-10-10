using System.Threading.Tasks;
using Fusion;
using InGame.Logic.Flow;
using Unit;

namespace InGame.Logic
{
    public class ClientPhaseEnd : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.End;

        public override async Task OnEnter()
        {
            if (Player.RegistedUsers.TryGetValue(runner.LocalPlayer, out var user))
                runner.Despawn(user.Object);

            onPhaseReport?.Invoke();
            await Task.CompletedTask;
        }
    }
}