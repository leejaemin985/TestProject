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
                phase.Initialize();
                phaseMap.Add(phase.phaseType, phase);
            }

            await SetPhase(new() { phaseType = FlowPhase.Init });
        }

        private async Task SetPhase(PhaseDirective phaseDirective)
        {
            //Exit Phase
            var exitPhase = currentPhase ?? ClientPhaseNone.Instance;
            ReportFromPhase(exitPhase.phaseType, await exitPhase.OnExit());


            if (phaseMap.TryGetValue(phaseDirective.phaseType, out IClientPhase targetPhase))
                currentPhase = targetPhase;
            else
                currentPhase = ClientPhaseNone.Instance;

            //Enter Phase
            ReportFromPhase(currentPhase.phaseType, await currentPhase.OnEnter(phaseDirective));

            //OnPhase
            ReportFromPhase(currentPhase.phaseType, await currentPhase.OnPhase());
        }

        public Task ApplyPhase(PhaseDirective directiveInfo) => SetPhase(directiveInfo);
    }
}