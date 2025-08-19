using InGame.Logic.Flow;
using System.Threading.Tasks;

namespace InGame.Logic
{
    public class ClientPhaseEnd : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.End;
    }
}