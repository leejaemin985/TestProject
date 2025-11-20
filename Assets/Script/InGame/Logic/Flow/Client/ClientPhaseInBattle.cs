using InGame.Logic.Flow;
using System;
using System.Linq.Expressions;
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
            if (Player.RegistedUsers.TryGetValue(runner.LocalPlayer, out var player))
                player.SetCanController(true);

            return Task.FromResult(PhaseState.Init);
        }

        protected async override Task<PhaseState> OnPhase()
        {
            Spinner.Instance.OffSpinner();

            bool aliveAllUser = true;
            const int CHECK_DELAY = 100;

            while (aliveAllUser)
            {
                await Task.Delay(CHECK_DELAY);

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
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return PhaseState.Error;
                }

            }
            Time.timeScale = .3f;
            return PhaseState.Complete;
        }

    }
}