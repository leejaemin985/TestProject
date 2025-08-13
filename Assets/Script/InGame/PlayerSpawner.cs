using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using Fusion;

using InGame.UI;
using Unit;
using UnityEngine.UIElements;

public class PlayerSpawner : SimulationBehaviour
{
    private NetworkRunner runner => GameNetworkManager.Instance.runner;

    [Header("Prefab")]
    [SerializeField] private PlayerRegistry PlayerRegistry;
    [SerializeField] private PhysicsEventHandler physicsEventHandler;
    [SerializeField] private EventDispatcher eventDispatcher;
    [SerializeField] private Player playerPrefab;


    [Header("Setting")]
    [SerializeField] private Transform[] spawnPos = default;

    public async Task Spawn()
        => await Task.WhenAll(SetMasterSingleton(), SpawnPlayer());

    private async Task SetMasterSingleton()
    {
        if (!runner.IsSharedModeMasterClient) return;

        List<Task> tasks = new()
        {
            SpawnPlayerRegister(),
            SpawnPhysicEventHandler(),
            SpawnEventDispatcher()
        };

        await Task.WhenAll(tasks);
    }

    private async Task SpawnPlayerRegister()
        => await runner.SpawnAsync(PlayerRegistry, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

    private async Task SpawnPhysicEventHandler()
        => await runner.SpawnAsync(physicsEventHandler, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

    private async Task SpawnEventDispatcher()
        => await runner.SpawnAsync(eventDispatcher, Vector3.zero, Quaternion.identity, runner.LocalPlayer);


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
