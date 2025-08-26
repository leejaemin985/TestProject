using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;

namespace InGame.Logic.Flow
{
    public class InGameClientPhaseAgent : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [SerializeField] private ClientPhaseBase[] clientPhaseList;
        private Dictionary<FlowPhase, IClientPhase> phaseMap;
        private IClientPhase currentPhase;

        private Action<PhaseReport> phaseReportAction;
        private Action<PlayerRef> phaseDoneAction;

        public async void Initialize(Action<PhaseReport> phaseReportAction, Action<PlayerRef> phaseDoneListener)
        {
            this.phaseReportAction = phaseReportAction;
            this.phaseDoneAction = phaseDoneListener;

            phaseMap = new();
            foreach (ClientPhaseBase phase in clientPhaseList)
            {
                phase.Initialize(UserPhaseDone);
                phaseMap.Add(phase.phaseType, phase);
            }

            await SetPhase(new() { phase = FlowPhase.Init });
        }

        private async Task SetPhase(PhaseDirective phaseDirective)
        {
            await (currentPhase?.OnExit() ?? Task.CompletedTask);

            currentPhase = phaseMap[phaseDirective.phase];
            phaseReportAction?.Invoke(new()
            {
                userRef = runner.LocalPlayer,
                phase = phaseDirective.phase
            });

            await (currentPhase?.OnEnter(phaseDirective) ?? Task.CompletedTask);
        }

        private void UserPhaseDone()
        {
            phaseDoneAction?.Invoke(runner.LocalPlayer);
        }

        public async void ApplyPhase(PhaseDirective directiveInfo)
        {
            await SetPhase(directiveInfo);
        }
    }
}