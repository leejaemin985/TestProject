using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unit;
using UnityEngine;

public class InGameMainLogic : SimulationBehaviour
{
    private NetworkRunner runner => GameNetworkManager.Instance.runner;

    public PlayerRegistry PlayerRegistry;
    public PhysicsEventHandler physicsEventHandler;
    public EventDispatcher eventDispatcher;
    public Player playerPrefab;

    // Start is called before the first frame update
    private async void Start()
    {
        bool masterClient = runner.IsSharedModeMasterClient;
        if (masterClient)
        {
            if (PlayerRegistry.Instance == null) 
                await runner.SpawnAsync(PlayerRegistry, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
            
            if (PhysicsEventHandler.Instance == null) 
                await runner.SpawnAsync(physicsEventHandler, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
            
            if (EventDispatcher.Instance == null) 
                await runner.SpawnAsync(eventDispatcher, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
        }

        await runner.SpawnAsync(
            playerPrefab,
            Vector3.up,
            Quaternion.identity,
            runner.LocalPlayer);

    }
}
