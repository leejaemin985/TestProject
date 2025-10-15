using Fusion;
using System.Threading.Tasks;

namespace InGame.Logic.Flow
{
    public sealed class ClientPhaseNone : ClientPhaseBase
    {
        public static IClientPhase Instance { get; private set; } = new ClientPhaseNone();

        public override FlowPhase phaseType => FlowPhase.None;

        //public FlowPhase phaseType => FlowPhase.None;
        //
        //public Task<PhaseReport> OnEnter(PhaseDirective phaseDirective) => Task.FromResult(InValidPhaseReport);
        //
        //public Task<PhaseReport> OnExit() => Task.FromResult(InValidPhaseReport);
        //
        //public PhaseState OnTick(float deltaTime) => PhaseState.None;
    }
}