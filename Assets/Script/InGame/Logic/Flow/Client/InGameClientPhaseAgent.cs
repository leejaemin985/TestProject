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
        private IClientPhase currentPhase;

        private Action<PhaseReport> phaseReportAction;

        public async void Initialize(Action<PhaseReport> phaseReportAction)
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
            var exitReport = await (currentPhase?.OnExit() ?? ClientPhaseNone.Instance.OnExit());
            phaseReportAction?.Invoke(exitReport);

            currentPhase = phaseMap[phaseDirective.phaseType] ?? ClientPhaseNone.Instance;
            phaseReportAction?.Invoke(await currentPhase.OnEnter(phaseDirective));
        }

        public async void ApplyPhase(PhaseDirective directiveInfo)
        {
            await SetPhase(directiveInfo);
        }
    }
}