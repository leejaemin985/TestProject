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
        private float startTick;

        public override FlowPhase phaseType => FlowPhase.Intro;


        protected override Task<PhaseState> OnEnter(PhaseDirective phaseDirective)
        {
            startTick = phaseDirective.startTick;

            CheckWarmup();

            return Task.FromResult(PhaseState.Init);
        }

        private async void CheckWarmup()
        {
            bool isWarmUp = true;
            while (isWarmUp)
            {
                isWarmUp = runner.Tick < (WARMUP_TIME / runner.DeltaTime) + startTick;

                reportPhase?.Invoke(PhaseState.Run);

                await Task.Delay(100);
            }

            reportPhase?.Invoke(PhaseState.Wait);
        }
    }
}
