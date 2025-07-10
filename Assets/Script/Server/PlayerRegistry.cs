using System.Collections.Generic;
using Unit;
using UnityEngine;
using Fusion;


public class PlayerRegistry : MasterSingleton<PlayerRegistry>
{
    private Dictionary<PlayerRef, Player> registedUser;

    protected override void Initialize()
    {
        base.Initialize();

        registedUser = new();
    }

    public void RegisterPlayer(PlayerRef userRef, Player player)
    {
        if (registedUser.ContainsKey(userRef)) registedUser[userRef] = player;
        registedUser.Add(userRef, player);
    }

    public Player GetOtherPlayer(PlayerRef userRef)
    {
        foreach (var user in registedUser)
        {
            if (user.Value.userRef.Equals(userRef) == false) return user.Value;
        }
        return null;
    }
}
