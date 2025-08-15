using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace InGame.Logic.Flow
{
    public class InGameClientPhaseAgent : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [SerializeField] private ClientPhaseBase[] clientPhaseList;
        private Dictionary<FlowPhase, IClientPhase> phaseMap;
        private IClientPhase currentPhase;

        private Action<PhaseReport> phaseReportAction;

        public async void Initialize(Action<PhaseReport> phaseReportAction)
        {
            this.phaseReportAction = phaseReportAction;
            
            phaseMap = new();
            foreach (IClientPhase phase in clientPhaseList)
            {
                phaseMap.Add(phase.phaseType, phase);
            }

            await SetPhase(FlowPhase.Init);
        }

        private async Task SetPhase(FlowPhase phaseType)
        {
            await (currentPhase?.OnExit() ?? Task.CompletedTask);
            currentPhase = phaseMap[phaseType];
            await (currentPhase?.OnEnter() ?? Task.CompletedTask);

            phaseReportAction?.Invoke(new()
            {
                userRef=runner.LocalPlayer,
                phase = phaseType
            });
        }

        public async void ApplyPhase(PhaseDirective directiveInfo)
        {
            await SetPhase(directiveInfo.phase);
        }
    }
}