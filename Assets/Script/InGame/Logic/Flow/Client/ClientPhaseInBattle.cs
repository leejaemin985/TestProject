using InGame.Logic.Flow;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unit;
using UnityEngine;
using Utility.Spinner;

namespace InGame.Logic
{
    public class ClientPhaseInBattle : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.InBattle;

        protected override Task<PhaseState> OnEnter(PhaseDirective phaseDirective)
        {
            Spinner.Instance.OffSpinner();

            if (Player.RegistedUsers.TryGetValue(runner.LocalPlayer, out var player))
                player.SetCanController(true);

            InBattle();

            return Task.FromResult(PhaseState.Init);
        }

        private async void InBattle()
        {
            bool aliveAllUser = true;
            const int CHECK_DELAY = 1000;

            while (aliveAllUser)
            {
                try
                {
                    foreach (var user in Player.RegistedUsers.Values)
                    {
                        if (user.isAlive() == false)
                        {
                            aliveAllUser = false;
                            break;
                        }
                    }

                    reportPhase?.Invoke(PhaseState.Run);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    reportPhase?.Invoke(PhaseState.Error);
                }

                await Task.Delay(CHECK_DELAY);
            }


            reportPhase?.Invoke(PhaseState.Wait);
        }

    }
}