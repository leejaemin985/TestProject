using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WaitingRoomUserHandle : NetworkBehaviour
{
    private WaitingRoomLogic waitingRoomMainLogic = default;
    [Networked] public bool readyState { get; private set; }
    private Action onChangedReadyStateListener;

    public override void Spawned()
    {
        InitSequencer();
    }

    private async Task InitSequencer()
    {
        const int FIND_DELAY_MS = 100;

        while ((waitingRoomMainLogic = FindObjectOfType<WaitingRoomLogic>()) == null)
        {
            await Task.Delay(FIND_DELAY_MS);
        }

        waitingRoomMainLogic.RegisterUserHandle(Object.StateAuthority, this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.Log($"Test - is User: {Runner.LocalPlayer == Object.StateAuthority}");
        waitingRoomMainLogic.UnregisterUserHandle(Object.StateAuthority, this);
    }

    public void SetChangedReadyStateListener(Action eventListener) => onChangedReadyStateListener = eventListener;

    public void ChangedReadyState()
    {
        if (HasStateAuthority == false) return;

        readyState = !readyState;
        RPC_ChangeEntryStatus();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ChangeEntryStatus()
    {
        onChangedReadyStateListener?.Invoke();
    }
}
