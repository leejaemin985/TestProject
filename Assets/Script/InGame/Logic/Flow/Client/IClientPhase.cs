using Fusion;
using System.Threading.Tasks;

namespace InGame.Logic.Flow
{
    public interface IClientPhase
    {
        public FlowPhase phaseType { get; }

        public Task OnEnter();

        public Task OnExit();
    }
}