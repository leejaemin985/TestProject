using System.Threading;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unit;
using UnityEngine;
using System.Threading.Tasks;

public class InGameMainLogic : SimulationBehaviour
{
    private NetworkRunner runner => GameNetworkManager.Instance.runner;

    [Header("Prefab")]
    [SerializeField] private PlayerRegistry PlayerRegistry;
    [SerializeField] private PhysicsEventHandler physicsEventHandler;
    [SerializeField] private EventDispatcher eventDispatcher;
    [SerializeField] private Player playerPrefab;


    [Header("Setting")]
    [SerializeField] private Transform[] spawnPos = default;


    // Start is called before the first frame update
    private async void Start()
    {
        await SetMasterSingleton();

        await SpawnPlayer();
    }

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
            playerPrefab,
            spawnPos.position,
            spawnPos.rotation,
            runner.LocalPlayer);
    }

}
