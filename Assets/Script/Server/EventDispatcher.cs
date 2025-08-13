using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unit;
using Unity.VisualScripting;
using UnityEngine;

public class EventDispatcher : SessionSingleton<EventDispatcher>
{
    public void RequestOnHitUser(PlayerRef user, HitInfo hitInfo)
    {
        if (!HasStateAuthority) return;
        RPC_RequestOnHitUser(user, hitInfo);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestOnHitUser(PlayerRef user, HitInfo hitInfo)
    {
        var targetUser = PlayerRegistry.Instance.RegistedUsers.FirstOrDefault(x => x.Key.Equals(user)).Value;
        if (targetUser != null)
        {
            targetUser.RequestOnHitState(hitInfo);
        }
    }

    public void RequestOnParringUser(PlayerRef user, HitInfo hitInfo)
    {
        if (!HasStateAuthority) return;
        RPC_RequestOnParringUser(user, hitInfo);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestOnParringUser(PlayerRef user, HitInfo hitInfo)
    {
        var targetUser = PlayerRegistry.Instance.RegistedUsers.FirstOrDefault(x => x.Key.Equals(user)).Value;
        if (targetUser != null)
        {
            targetUser.RequestOnParringState(hitInfo);
        }
    }
}
