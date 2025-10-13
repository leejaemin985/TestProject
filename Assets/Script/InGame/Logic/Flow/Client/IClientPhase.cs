using Fusion;
using System.Threading.Tasks;

namespace InGame.Logic.Flow
{
    public interface IClientPhase
    {
        public FlowPhase phaseType { get; }

        public Task<PhaseReport> OnEnter(PhaseDirective phaseDirective);

        public Task<PhaseReport> OnExit();

        public PhaseState OnTick(float deltaTime);
    }
}