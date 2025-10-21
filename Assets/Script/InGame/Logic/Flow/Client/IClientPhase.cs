using Fusion;
using System.Threading.Tasks;

namespace InGame.Logic.Flow
{
    public interface IClientPhase
    {
        public FlowPhase phaseType { get; }

        public Task<PhaseState> OnEnter(PhaseDirective phaseDirective);

        public Task<PhaseState> OnExit();

        public Task<PhaseState> OnPhase();
    }
}