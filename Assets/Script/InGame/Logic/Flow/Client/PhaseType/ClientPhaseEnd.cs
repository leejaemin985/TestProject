using Fusion;
using InGame.Logic.Flow;
using System.Threading.Tasks;

namespace InGame.Logic
{
    public class ClientPhaseEnd : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.End;

        public override async Task OnEnter()
        {
            if (PlayerRegistry.Instance.RegistedUsers.TryGetValue(runner.LocalPlayer, out var user))
            {
                runner.Despawn(user.Object);
            }

            phaseDoneListener?.Invoke();
        }
    }
}