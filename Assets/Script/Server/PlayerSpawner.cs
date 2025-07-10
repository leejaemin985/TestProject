using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UIElements;
using System.Linq;
using Unit;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public PlayerRegistry PlayerRegistry;
    public PhysicsEventHandler physicsEventHandler;
    public Player playerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        bool masterClient = Runner.IsSharedModeMasterClient;
        if (masterClient)
        {
            Runner.Spawn(PlayerRegistry, Vector3.zero, Quaternion.identity, player);
            Runner.Spawn(physicsEventHandler, Vector3.zero, Quaternion.identity, player);
        }

        if (player != Runner.LocalPlayer) return;

        Runner.Spawn(
            playerPrefab,
            Vector3.up,
            Quaternion.identity,
            player,
            (runner, obj) =>
            {
                var user = obj.GetComponent<Player>();
                if (user == null) return;

                user.PreSpawnInitialize(player);
            });


    }
}
