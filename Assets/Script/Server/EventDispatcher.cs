using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unit;
using UnityEngine;

public class EventDispatcher : MasterSingleton<EventDispatcher>
{
    public void SetStateEvent(PlayerRef user, PlayerFSM.StateType stateType)
    {
        if (!HasStateAuthority) return;
        RPC_UserSetStateEvent(user, stateType);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UserSetStateEvent(PlayerRef user, PlayerFSM.StateType stateType)
    {
        var targetUser = PlayerRegistry.Instance.RegistedUsers.FirstOrDefault(x => x.Key.Equals(user)).Value;
        if (targetUser != null)
        {
            targetUser.SetStateEvent(stateType);
        }
    }
}
