using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.Spinner;
using System.Threading;

namespace InGame.Logic.Flow
{
    public class ClientPhaseWarmup : ClientPhaseBase
    {
        public const int WARMUP_TIME = 3;

        public override FlowPhase phaseType => FlowPhase.Intro;

        private Task waitTask;
        private CancellationTokenSource cts;

        public override async Task<PhaseReport> OnEnter(PhaseDirective phaseDirective)
        {
            //if (waitTask != null && !waitTask.IsCompleted) return GetValidPhaseReport(PhaseState.Run);
        
        }

        public override PhaseReport OnPhase()
        {
            var completeTick = (WARMUP_TIME / runner.DeltaTime) + phaseDirective.startTick;
            if (runner.Tick < completeTick) return GetValidPhaseReport(PhaseState.Init);

            return GetValidPhaseReport(PhaseState.Wait);
        }
    }
}
