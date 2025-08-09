using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameNetworkManager : MonoSingleton<GameNetworkManager>
{
    public bool isInitialized { get; private set; } = false;

    private const string RUNNER_PATH = "Network/Prototype Runner";

    public NetworkRunner runner { get; private set; }

    private List<SessionInfo> currentSessionInfo;
    public IReadOnlyList<SessionInfo> sessionList => currentSessionInfo;

    public async Task Initialize()
    {
        if (runner == null)
        {
            runner = Instantiate(Resources.Load<NetworkRunner>(RUNNER_PATH));
            DontDestroyOnLoad(runner);
        }

        var connect = await runner.JoinSessionLobby(SessionLobby.ClientServer, null);
        if (connect.Ok == false)
        {
            // 예외처리
            return;
        }

        var sessionReceiver = runner.GetComponent<SessionListReceiver>();
        sessionReceiver.SetSessionUpdateListener(UpdateSessionList);

        isInitialized = true;
    }

    private void UpdateSessionList(List<SessionInfo> newList)
    {
        currentSessionInfo = newList;
    }

}
