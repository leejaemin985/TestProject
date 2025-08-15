using Fusion;
using InGame.Logic.Flow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Logic
{
    public class InGameEntryPoint : SimulationBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [SerializeField] private PhaseSequencer phaseSequencerPrefab;


        public async void Start()
        {
            if (!runner.IsSharedModeMasterClient) return;

            await runner.SpawnAsync(phaseSequencerPrefab);
        }


    }
}
