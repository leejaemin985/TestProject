using System.Collections.Generic;
using Unit;
using UnityEngine;
using Fusion;


public class PlayerRegistry : SessionSingleton<PlayerRegistry>
{
    private Dictionary<PlayerRef, Player> registedUsers;

    public IReadOnlyDictionary<PlayerRef,Player> RegistedUsers => registedUsers;

    protected override void Initialize()
    {
        registedUsers = new();

        isInitialized = true;
    }


    public void RegisterPlayer(PlayerRef userRef, Player player)
    {
        if (registedUsers.ContainsKey(userRef)) return;
        registedUsers.Add(userRef, player);
    }

    public void UnRegisterPlayer(PlayerRef userRef)
    {
        registedUsers.Remove(userRef);
    }
}
