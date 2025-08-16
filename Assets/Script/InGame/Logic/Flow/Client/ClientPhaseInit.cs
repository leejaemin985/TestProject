using System.Threading.Tasks;

namespace InGame.Logic.Flow
{
    public class ClientPhaseInit : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.Init;

        public override async Task OnEnter()
        {
            phaseDoneListener?.Invoke();
        }
    }
}