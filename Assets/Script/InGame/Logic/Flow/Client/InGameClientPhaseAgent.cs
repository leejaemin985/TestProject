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

        bool isTransition = false;

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

            Test();
        }

        private async Task SetPhase(PhaseDirective phaseDirective)
        {
            isTransition = true;

            var exitReport = await (currentPhase?.OnExit() ?? ClientPhaseNone.Instance.OnExit());
            phaseReportAction?.Invoke(exitReport);

            currentPhase = phaseMap[phaseDirective.phaseType] ?? ClientPhaseNone.Instance;
            phaseReportAction?.Invoke(await currentPhase.OnEnter(phaseDirective));

            isTransition = false;
        }

        public async void ApplyPhase(PhaseDirective directiveInfo)
        {
            await SetPhase(directiveInfo);
        }

        private async void Test()
        {
            while (true)
            {
                await Task.Delay(500);

                if (isTransition) return;

                phaseReportAction?.Invoke(new()
                {
                    userRef = Runner.LocalPlayer,
                    phaseType = currentPhase.phaseType,
                    phaseState = currentPhase.OnTick(Runner.DeltaTime)
                });
            }
        }

        public override void FixedUpdateNetwork()
        {
            //if (isTransition) return;
            //
            //phaseReportAction?.Invoke(new()
            //{
            //    userRef = Runner.LocalPlayer,
            //    phaseType = currentPhase.phaseType,
            //    phaseState = currentPhase.OnTick(Runner.DeltaTime)
            //});
        }
    }
}