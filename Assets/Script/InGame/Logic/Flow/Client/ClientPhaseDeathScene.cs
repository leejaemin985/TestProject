using InGame.Logic.Flow;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Logic
{
    public class ClientPhaseDeathScene : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.DeathScene;

        protected async override Task<PhaseState> OnPhase()
        {
            await Task.Delay(5000);

            Time.timeScale = 1;
            return PhaseState.Complete;
        }

    }
}