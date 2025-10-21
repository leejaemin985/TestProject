using System;
using System.Threading.Tasks;
using UnityEngine;

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
            return Task.FromResult(PhaseState.Init);
        }

        protected async override Task<PhaseState> OnPhase()
        {
            try
            {
                while (runner.Tick < (WARMUP_TIME / runner.DeltaTime) + startTick)
                {
                    await Task.Delay(100);
                }

                return PhaseState.Wait;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return PhaseState.Error;
            }
        }
    }
}
