using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UIElements;
using System.Linq;
using Unit;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public EventHandler eventHandler;
    public Player playerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        bool firstJoined = Runner.ActivePlayers.Count() == 1;
        if (firstJoined)
        {
            Runner.Spawn(eventHandler, Vector3.zero, Quaternion.identity, player);
            EventHandler.SetServer(true);
        }
        else if (player == Runner.LocalPlayer)
        {
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
            EventHandler.SetServer(false);
        }
    }
}
