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


        protected override Task<PhaseReport> OnEnter(PhaseDirective phaseDirective)
        {
            startTick = phaseDirective.startTick;
            return Task.FromResult(CreatePhaseReport(PhaseState.Init));
        }

        protected override PhaseState OnTick(float deltaTime)
        {
            var completeTick = (WARMUP_TIME / runner.DeltaTime) + startTick;
            PhaseState phaseState = runner.Tick < completeTick ? PhaseState.Run : PhaseState.Wait;

            return phaseState;
        }
    }
}
