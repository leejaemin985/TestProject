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

        [Header("Setting")]
        [SerializeField] private Transform[] spawnPos = default;


        public async Task Spawn()
            => await SpawnPlayer();


        private async Task SpawnPlayer()
        {
            Transform spawnPos = this.spawnPos[runner.IsSharedModeMasterClient ? 0 : 1];

            await runner.SpawnAsync(
                prefab: playerPrefab,
                position: spawnPos.position,
                rotation: spawnPos.rotation,
                inputAuthority: runner.LocalPlayer,
                onCompleted: (result) => UnityEngine.Debug.Log("Spawned Done"));

        }

    }
}