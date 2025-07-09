using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Unit;

public class PhysicsEventHandler : NetworkBehaviour
{
    public static bool isSpawned { get; private set; }

    private static PhysicsEventHandler instance;
    public static PhysicsEventHandler Instance => instance;

    private Dictionary<PlayerRef, Player> registedUser;

    public override void Spawned()
    {
        instance = this;
        registedUser = new();

        isSpawned = true;
    }

    public void RegisterPlayer(PlayerRef userRef, Player player)
    {
        if (registedUser.ContainsKey(userRef)) registedUser[userRef] = player;
        registedUser.Add(userRef, player);
    }


}
