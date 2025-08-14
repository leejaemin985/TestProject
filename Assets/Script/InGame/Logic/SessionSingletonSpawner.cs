using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using Fusion;

public class SessionSingletonSpawner : SimulationBehaviour
{
    private NetworkRunner runner => GameNetworkManager.Instance.runner;
    private bool IsAuthority => runner.IsSharedModeMasterClient;

    [Header("Prefab")]
    [SerializeField] private PlayerRegistry PlayerRegistry;
    [SerializeField] private PhysicsEventHandler physicsEventHandler;
    [SerializeField] private EventDispatcher eventDispatcher;

    public async Task SpawnAsync()
        => await (IsAuthority ? EnsureAuthoritySingletonsAsync() : WaitForAuthoritySingletonsAsync());

    private async Task EnsureAuthoritySingletonsAsync()
    {
        List<Task> tasks = new()
        {
            SpawnPlayerRegisterAsync(),
            SpawnPhysicEventHandlerAsync(),
            SpawnEventDispatcherAsync()
        };

        await Task.WhenAll(tasks);
    }
    
    private async Task WaitForAuthoritySingletonsAsync()
    {
        const int CHECK_DELAY_MS = 100;

        bool CheckRegisteryInitialize() => PlayerRegistry.Instance != null && PlayerRegistry.Instance.Initialized;
        bool CheckPhysicEventHandlerInitialize() => PhysicsEventHandler.Instance != null && PhysicsEventHandler.Instance.Initialized;
        bool CheckEventDispatcherInitialize() => EventDispatcher.Instance != null && EventDispatcher.Instance.Initialized;

        while (true)
        {
            if (CheckRegisteryInitialize() &&
                CheckPhysicEventHandlerInitialize() &&
                CheckEventDispatcherInitialize())
                break;

            await Task.Delay(CHECK_DELAY_MS);
        }
    }


    private async Task SpawnPlayerRegisterAsync()
        => await runner.SpawnAsync(PlayerRegistry, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

    private async Task SpawnPhysicEventHandlerAsync()
        => await runner.SpawnAsync(physicsEventHandler, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

    private async Task SpawnEventDispatcherAsync()
        => await runner.SpawnAsync(eventDispatcher, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

}
