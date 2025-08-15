using System.Threading.Tasks;

using UnityEngine;
using Unit;

namespace InGame.Logic.Flow
{
    public class ClientPhaseSpawn : ClientPhaseBase
    {
        public override FlowPhase phaseType => FlowPhase.Spawn;


        [Header("Spawner")]
        [SerializeField] private SessionSingletonSpawner sessionSingletonSpawner;
        [SerializeField] private PlayerSpawner playerSpawner;

        public override async Task OnEnter()
        {
            await Spawn();
        }

        private async Task Spawn()
        {
            await sessionSingletonSpawner.SpawnAsync();

            await playerSpawner.SpawnAsync();
        }

    }
}