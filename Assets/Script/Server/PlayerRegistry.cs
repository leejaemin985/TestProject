using System.Collections.Generic;
using Unit;
using UnityEngine;
using Fusion;


public class PlayerRegistry : MasterSingleton<PlayerRegistry>
{
    private Dictionary<PlayerRef, Player> registedUsers = new();
    private List<Player> registerTest = new();

    public IReadOnlyDictionary<PlayerRef,Player> RegistedUsers => registedUsers;

    protected override void Initialize()
    {
        base.Initialize();
    }

    public void RegisterPlayer(PlayerRef userRef, Player player)
    {
        if (registedUsers.ContainsKey(userRef)) return;
        registedUsers.Add(userRef, player);
    }
}
