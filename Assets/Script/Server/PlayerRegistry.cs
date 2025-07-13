using System.Collections.Generic;
using Unit;
using UnityEngine;
using Fusion;


public class PlayerRegistry : MasterSingleton<PlayerRegistry>
{
    private Dictionary<PlayerRef, Player> registedUsers;

    public IReadOnlyDictionary<PlayerRef,Player> RegistedUsers => registedUsers;

    protected override void Initialize()
    {
        base.Initialize();

        registedUsers = new();
    }

    public void RegisterPlayer(PlayerRef userRef, Player player)
    {
        if (registedUsers.ContainsKey(userRef)) registedUsers[userRef] = player;
        registedUsers.Add(userRef, player);
    }
}
