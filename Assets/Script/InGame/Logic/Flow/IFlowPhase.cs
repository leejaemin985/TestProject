using System;
using System.Threading.Tasks;

namespace InGame.Logic.Flow
{
    interface IFlowPhase
    {
        FlowPhase PhaseType { get; }

        Task Initialize(Action phaseEnterListener, Action phaseDoneListener);

        Task EnterPhase();

        void OnPhase();

        Task ExitPhase();
    }
}