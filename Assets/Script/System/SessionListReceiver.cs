using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionListReceiver : RunnerCallbacksBase
{
    private Action<List<SessionInfo>> onChangedSeesionListListener;

    public void SetSessionUpdateListener(Action<List<SessionInfo>> newListener) => onChangedSeesionListListener = newListener;

    protected override void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        onChangedSeesionListListener?.Invoke(sessionList);
    }
}
