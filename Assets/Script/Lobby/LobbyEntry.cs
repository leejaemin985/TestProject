using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyEntry : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    private NetworkRunner runner;

    private async void Start()
    {
        runner = Instantiate(runnerPrefab);
        var sessionReceiver = runner.GetComponent<SessionListReceiver>();
        sessionReceiver.SetSessionUpdateListener(UpdateSessionList);

        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer, null);
    }

    private void UpdateSessionList(List<SessionInfo> sessionList)
    {

    }
}
