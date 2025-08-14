using System.Threading;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unit;
using UnityEngine;
using System.Threading.Tasks;
using InGame.UI;

namespace InGame.Logic
{
    public class InGameMainLogic : SimulationBehaviour
    {
        [Header("Network")]
        [SerializeField] private SessionSingletonSpawner spawner;
        [SerializeField] private PlayerSpawner playerSpawner;


        [Header("Setting")]
        [SerializeField] private InGameBattleUI battleUI;

        private void Start() => Entry();

        private async void Entry()
        {
            await spawner.SpawnAsync();

            await playerSpawner.SpawnAsync();
        }
    }
}
