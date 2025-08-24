using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unit;
using Unity.VisualScripting;
using UnityEngine;

public class EventDispatcher : SessionSingleton<EventDispatcher>
{
    public void RequestOnHitUser(PlayerRef userRef, HitInfo hitInfo)
    {
        if (!HasStateAuthority) return;
        RPC_RequestOnHitUser(userRef, hitInfo);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestOnHitUser(PlayerRef userRef, HitInfo hitInfo)
    {
        if (PlayerRegistry.Instance.RegistedUsers.TryGetValue(userRef, out Player user))
        {
            user.RequestOnHitState(hitInfo);
        }
    }

    public void RequestOnParringUser(PlayerRef userRef, HitInfo hitInfo)
    {
        if (!HasStateAuthority) return;
        RPC_RequestOnParringUser(userRef, hitInfo);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestOnParringUser(PlayerRef userRef, HitInfo hitInfo)
    {
        if (PlayerRegistry.Instance.RegistedUsers.TryGetValue(userRef, out Player user))
        {
            user.RequestOnParringState(hitInfo);
        }
    }

    public void RequestOnDiedUser(PlayerRef userRef, HitInfo hitInfo)
    {
        if (!HasStateAuthority) return;
        RPC_RequestOnDiedUser(userRef, hitInfo);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestOnDiedUser(PlayerRef userRef, HitInfo hitInfo)
    {
        if (PlayerRegistry.Instance.RegistedUsers.TryGetValue(userRef, out Player user))
        {
            user.RequestOnDiedState(hitInfo);
        }
    }
}
