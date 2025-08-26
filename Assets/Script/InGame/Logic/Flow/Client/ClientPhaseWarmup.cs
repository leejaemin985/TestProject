using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Logic.Flow
{
    public class ClientPhaseWarmup : ClientPhaseBase
    {
        public const int WARMUP_TIME = 3;

        public override FlowPhase phaseType => FlowPhase.Warmup;

        public override async Task OnEnter()
        {
            var completeTick = (WARMUP_TIME / runner.DeltaTime) + phaseDirective.startTick;

            while (runner.Tick < completeTick) await Task.Yield();
            phaseDoneListener?.Invoke();
        }
    }
}
