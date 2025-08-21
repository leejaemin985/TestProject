using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameNetworkManager : MonoSingleton<GameNetworkManager>
{
    public enum NetLifecycleState
    {
        Disconnected,      // Runner 없음 or 종료됨
        LobbyConnected,    // 로비 접속
        InSession,         // 세션 안
    }

    public NetLifecycleState NetState { get; private set; } = NetLifecycleState.Disconnected;

    public bool isInitialized { get; private set; } = false;

    private const string RUNNER_PATH = "Network/Prototype Runner";

    public NetworkRunner runner { get; private set; }


    private List<SessionInfo> currentSessionInfo;
    public List<SessionInfo> sessionList => currentSessionInfo;
    private Action<List<SessionInfo>> onEventSessionUpdateListener { get; set; }


    private List<PlayerRef> currentConnectedUsers;
    public List<PlayerRef> connectedUsers => currentConnectedUsers;
    private Action<PlayerRef> onEventJoinedUserListener { get; set; }
    private Action<PlayerRef> onEventLeftUserListener { get; set; }

    private Action<ShutdownReason> onEventShutDownListener { get; set; }
    

    public async Task Connect()
    {
        Clear();

        if (runner != null)
        {
            await runner.Shutdown();
            Destroy(runner);
        }

        runner = Instantiate(Resources.Load<NetworkRunner>(RUNNER_PATH));
        DontDestroyOnLoad(runner);

        var joinLobby = await runner.JoinSessionLobby(SessionLobby.ClientServer, null);
        if (joinLobby.Ok == false)
        {
            return;
        }

        NetState = NetLifecycleState.LobbyConnected;

        var sessionReceiver = runner.GetComponent<SessionListReceiver>();
        sessionReceiver.SetSessionUpdateListener(UpdateSessionList);

        var userConnectionObserver = runner.GetComponent<UserConnectionObserver>();
        userConnectionObserver.SetJoinedUserEventListener(OnJoinedUser);
        userConnectionObserver.SetLeftUserEventListener(OnLeftUser);

        var localShutDownObserver = runner.GetComponent<LocalConnectionObserver>();
        localShutDownObserver.SetShutDownEventListener(OnLocalShutDown);

        isInitialized = true;
    }

    #region Lobby
    
    public bool CanEnterSession(SessionInfo info)
    {
        return 
            info.IsValid &&
            info.IsVisible &&
            info.PlayerCount < info.MaxPlayers;
    }

    public void SetSessionUpdateEventListener(Action<List<SessionInfo>> eventListener)
    {
        onEventSessionUpdateListener -= eventListener;
        onEventSessionUpdateListener += eventListener;
    }

    private void UpdateSessionList(List<SessionInfo> newList)
    {
        currentSessionInfo = newList;
        onEventSessionUpdateListener?.Invoke(currentSessionInfo);
    }

    #endregion

    #region Session

    public void AddJoinedUserEventListener(Action<PlayerRef> eventListener)
    {
        onEventJoinedUserListener -= eventListener;
        onEventJoinedUserListener += eventListener;
    }

    public void AddLeftUserEventListener(Action<PlayerRef> eventListener)
    {
        onEventLeftUserListener -= eventListener;
        onEventLeftUserListener += eventListener;
    }

    private void OnJoinedUser(PlayerRef playerRef)
    {
        if (playerRef == runner.LocalPlayer)
        {
            NetState = NetLifecycleState.InSession;
        }

        if (currentConnectedUsers == null) currentConnectedUsers = new();

        currentConnectedUsers.Add(playerRef);
        onEventJoinedUserListener?.Invoke(playerRef);
    }

    private void OnLeftUser(PlayerRef playerRef)
    {
        if (currentConnectedUsers == null) currentConnectedUsers = new();

        currentConnectedUsers.Remove(playerRef);
        onEventLeftUserListener?.Invoke(playerRef);
    }

    #endregion

    #region Connection

    public void SetLocalShutDownEventListener(Action<ShutdownReason> eventListener)
    {
        onEventShutDownListener = eventListener;
    }

    private void OnLocalShutDown(ShutdownReason reason)
    {
        Clear();
        NetState = NetLifecycleState.Disconnected;

        onEventShutDownListener?.Invoke(reason);
    }

    #endregion

    private void Clear()
    {
        isInitialized = false;

        currentSessionInfo = null;
        currentConnectedUsers = null;

        onEventSessionUpdateListener = null;
        onEventJoinedUserListener = null;
        onEventLeftUserListener = null;
        onEventShutDownListener = null;
    }
}
