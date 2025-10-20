using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;

namespace InGame.Logic.Flow
{
    public class InGameClientPhaseAgent : NetworkBehaviour
    {
        [SerializeField] private ClientPhaseBase[] clientPhaseList;

        private Dictionary<FlowPhase, IClientPhase> phaseMap;
        private IClientPhase currentPhase = ClientPhaseNone.Instance;

        private Action<PhaseReport> phaseReportAction;

        private void ReportFromPhase(FlowPhase phaseType, PhaseState state)
        {
            phaseReportAction?.Invoke(new()
            {
                userRef = Runner.LocalPlayer,
                phaseType = phaseType,
                phaseState = state
            });
        }

        public async Task Initialize(Action<PhaseReport> phaseReportAction)
        {
            this.phaseReportAction = phaseReportAction;

            phaseMap = new();
            foreach (ClientPhaseBase phase in clientPhaseList)
            {
                phase.Initialize(state => ReportFromPhase(phase.phaseType, state));
                phaseMap.Add(phase.phaseType, phase);
            }

            await SetPhase(new() { phaseType = FlowPhase.Init });
        }

        private async Task SetPhase(PhaseDirective phaseDirective)
        {
            var exitPhase = currentPhase ?? ClientPhaseNone.Instance;
            var exitState = await exitPhase.OnExit();
            ReportFromPhase(exitPhase.phaseType, exitState);


            if (phaseMap.TryGetValue(phaseDirective.phaseType, out IClientPhase targetPhase))
                currentPhase = targetPhase;
            else
                currentPhase = ClientPhaseNone.Instance;

            var enterState = await currentPhase.OnEnter(phaseDirective);
            ReportFromPhase(currentPhase.phaseType, enterState);
        }

        public Task ApplyPhase(PhaseDirective directiveInfo) => SetPhase(directiveInfo);
    }
}