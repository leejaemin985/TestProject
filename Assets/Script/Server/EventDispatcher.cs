using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventDispatcher : MasterSingleton<EventDispatcher>
{
    public void SetStateEvent(PlayerRef user, string stateName)
    {
        if (!HasStateAuthority) return;
        RPC_UserSetStateEvent(user, stateName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UserSetStateEvent(PlayerRef user, string stateName)
    {
        var targetUser = PlayerRegistry.Instance.RegistedUsers.FirstOrDefault(x => x.Key.Equals(user)).Value;
        if (targetUser != null)
        {
            targetUser.SetStateEvent(stateName);
        }
    }
}
