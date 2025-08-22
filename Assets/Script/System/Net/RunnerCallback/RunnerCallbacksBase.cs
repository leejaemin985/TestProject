using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RunnerCallbacksBase : MonoBehaviour, INetworkRunnerCallbacks
{
    protected virtual void OnConnectedToServer(NetworkRunner runner) { }
    protected virtual void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    protected virtual void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    protected virtual void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    protected virtual void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    protected virtual void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    protected virtual void OnInput(NetworkRunner runner, NetworkInput input) { }
    protected virtual void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    protected virtual void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    protected virtual void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    protected virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    protected virtual void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    protected virtual void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    protected virtual void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    protected virtual void OnSceneLoadDone(NetworkRunner runner) { }
    protected virtual void OnSceneLoadStart(NetworkRunner runner) { }
    protected virtual void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    protected virtual void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    protected virtual void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    #region INetworkRunnerCallbacks Interface
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
        => OnConnectedToServer(runner);

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        => OnConnectFailed(runner, remoteAddress, reason);

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        => OnConnectRequest(runner, request, token);

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        => OnCustomAuthenticationResponse(runner, data);

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        => OnDisconnectedFromServer(runner, reason);

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        => OnHostMigration(runner, hostMigrationToken);

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
        => OnInput(runner, input);

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        =>OnInputMissing(runner, player, input);

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        => OnObjectEnterAOI(runner, obj, player);

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        => OnObjectExitAOI(runner, obj, player);

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        =>OnPlayerJoined(runner, player);

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        => OnPlayerLeft(runner, player);

    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        => OnReliableDataProgress(runner, player, key, progress);

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        => OnReliableDataReceived(runner, player, key, data);

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
        => OnSceneLoadDone(runner);

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
        => OnSceneLoadStart(runner);

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        => OnSessionListUpdated(runner, sessionList);

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        =>OnShutdown(runner, shutdownReason);

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) 
        => OnUserSimulationMessage(runner, message);
    #endregion
}
