using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject playerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Runner.Spawn(playerPrefab, Vector3.up, Quaternion.identity, player);
        }
    }
}
