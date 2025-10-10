using Fusion;
using System.Threading.Tasks;

namespace InGame.Logic.Flow
{
    public sealed class ClientPhaseNone : IClientPhase
    {
        private static PhaseReport InValidPhaseReport = new()
        {
            userRef = PlayerRef.None,
            phaseType = FlowPhase.None,
            phaseState = PhaseState.None
        };

        public static ClientPhaseNone Instance { get; private set; } = new();


        public FlowPhase phaseType => FlowPhase.None;

        public Task<PhaseReport> OnEnter(PhaseDirective phaseDirective) => Task.FromResult(InValidPhaseReport);

        public Task<PhaseReport> OnExit() => Task.FromResult(InValidPhaseReport);

        public PhaseReport OnPhase() => InValidPhaseReport;
    }
}