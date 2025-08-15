using Fusion;
using System.Threading.Tasks;
using Unit;
using UnityEngine;

namespace InGame.Logic
{
    public class PlayerSpawner : SimulationBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [Header("Prefab")]
        [SerializeField] private Player playerPrefab;

        [Header("SpawnPoints")]
        [SerializeField] private Transform[] spawnPoints = default;

        private bool spawned = false;

        public async Task SpawnAsync()
            => await SpawnPlayer();


        private async Task SpawnPlayer()
        {
            if (spawned) return;
            spawned = true;

            //Transform spawnPos = this.spawnPoints[runner.IsSharedModeMasterClient ? 0 : 1];
            Transform spawnPos = transform;

            await runner.SpawnAsync(
                prefab: playerPrefab,
                position: spawnPos.position,
                rotation: spawnPos.rotation,
                inputAuthority: runner.LocalPlayer);

        }

    }
}