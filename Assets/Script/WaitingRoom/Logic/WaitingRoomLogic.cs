using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Spinner;
using WaitingRoom.Net;

public class WaitingRoomLogic : MonoBehaviour
{
    private NetworkRunner runner => GameNetworkManager.Instance.runner;

    [SerializeField] private WaitingRoomUI uiHandle;

    [SerializeField] private GameObject userModel;
    [SerializeField] private GameObject opponentModel;

    [SerializeField] private WaitingRoomUserStateHandler stateHandlerPrefab;

    private WaitingRoomUserStateHandler userStateHandler;
    private WaitingRoomUserStateHandler opponentStateHandler;

    private void Start()
    {
        Debug.Log($"Waiting Room - Entry");

        if (runner.IsSharedModeMasterClient)
        {
            runner.SessionInfo.IsOpen = true;
        }

        foreach (var user in GameNetworkManager.Instance.connectedUsers)
        {
            WaitingRoomUserStateHandler.AddSpawnedListener(user, RegistUserStateHandler);
        }

        SpawnStateHandler();
        SetNetListener();

        UIInitialize();
        RefreshWaitingRoom();
    }

    private void UIInitialize()
    {
        uiHandle.onClickedGameEntryButtonListener = GameEntry;
        uiHandle.onClickedExitButtonListener = ExitSession;
    }

    private async void SpawnStateHandler()
    {
        Debug.Log($"Waiting Room - Spawn StateHandler");
        await runner.SpawnAsync(prefab: stateHandlerPrefab);
    }

    private void SetNetListener()
    {
        GameNetworkManager.Instance.AddJoinedUserEventListener(UserConnectingListener);
        GameNetworkManager.Instance.AddLeftUserEventListener(UserDisconnectingListener);
    }

    private void UserConnectingListener(PlayerRef userRef)
    {
        Debug.Log($"Waiting Room - User Connect {userRef}");

        WaitingRoomUserStateHandler.AddSpawnedListener(userRef, RegistUserStateHandler);
        
        RefreshWaitingRoom();
    }

    private void UserDisconnectingListener(PlayerRef userRef)
    {
        Debug.Log($"Waiting Room - User Disonnect {userRef}");

        WaitingRoomUserStateHandler.RemoveSpanwedListener(userRef);
        UnregistUser(userRef);

        RefreshWaitingRoom();
    }

    private void GameEntry()
    {
        if (userStateHandler != null) userStateHandler.SetReadyState(!userStateHandler.readyState);
    }

    private async void ExitSession()
    {
        bool isExitRequest = true;

        if (userStateHandler != null)
            runner.Despawn(userStateHandler.Object);

        Spinner.Instance.OnSpinner(() => isExitRequest == false);
        await GameNetworkManager.Instance.runner.Shutdown();
        isExitRequest = false;

        SceneManager.LoadScene(SceneType.SceneType.Localinitialize.id, LoadSceneMode.Single);
    }

    private void RegistUserStateHandler(WaitingRoomUserStateHandler userStateHandler)
    {
        if (userStateHandler == null)
        {
            Debug.LogWarning("Waiting Room - Skipped Regist UserStateHandler");
            return;
        }
        Debug.Log($"Waiting Room - Regist Spawned UserStateHandler(ref: {userStateHandler.Object.StateAuthority})");

        var authority = userStateHandler.Object.StateAuthority;
        if (authority == runner.LocalPlayer)
        {
            this.userStateHandler = userStateHandler;
            userStateHandler.AddChangedReadyStateListener(SetUserReadyState);
        }
        else
        {
            this.opponentStateHandler = userStateHandler;
            userStateHandler.AddChangedReadyStateListener(SetOpponentReadyState);
        }

        RefreshWaitingRoom();
    }

    private void UnregistUser(PlayerRef userRef)
    {
        if (userRef == runner.LocalPlayer) return;
        
        opponentStateHandler = null;
        uiHandle.SetOpponentReadyState(false);
    }

    private void SetUserReadyState(bool set)
    {
        Debug.Log($"Waiting Room - User Ready State: {set}");

        uiHandle.SetGameEntryButton(set);
        CheckStartGame();
    }

    private void SetOpponentReadyState(bool set)
    {
        Debug.Log($"Waiting Room - Opponent Ready State: {set}");

        uiHandle.SetOpponentReadyState(set);
        CheckStartGame();
    }

    private void RefreshWaitingRoom()
    {
        userModel.SetActive(userStateHandler != null);
        opponentModel.SetActive(opponentStateHandler != null);

        uiHandle.SetOpponentSlotActive(opponentStateHandler != null);
    }

    private void CheckStartGame()
    {
        if (runner.IsSharedModeMasterClient == false) return;

        bool fullSession = userStateHandler != null && opponentStateHandler != null;

        bool allUsersReady =
            fullSession &&
            userStateHandler.readyState && opponentStateHandler.readyState;

        if (fullSession && allUsersReady)
        {
            Debug.Log($"Waiting Room - Start Game");

            runner.LoadScene(SceneRef.FromIndex(SceneType.SceneType.InGame.id), LoadSceneMode.Single);
            runner.SessionInfo.IsOpen = false;
        }
    }
}
