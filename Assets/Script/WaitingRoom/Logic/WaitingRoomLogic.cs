using UnityEngine;
using UnityEngine.SceneManagement;

using Fusion;

using SceneType;
using Utility.Spinner;
using WaitingRoom.Net;

public class WaitingRoomLogic : MonoBehaviour
{
    private NetworkRunner runner => GameNetworkManager.Instance.runner;

    [SerializeField] private WaitingRoomUI uiHandle;

    [SerializeField] private GameObject userModel;
    [SerializeField] private GameObject opponentModel;

    [SerializeField] private WaitingRoomUserStateHandler stateHandlerPrefab;

    [SerializeField] private WaitingRoomUserStateHandler userStateHandler;
    [SerializeField] private WaitingRoomUserStateHandler opponentStateHandler;

    private async void Start()
    {
        if (runner.IsSharedModeMasterClient)
            runner.SessionInfo.IsOpen = true;

        await runner.SpawnAsync(prefab: stateHandlerPrefab);

        foreach (var user in runner.ActivePlayers)
        {
            WaitingRoomUserStateHandler.AddSpawnedCallback(user, RegistStateHandler);
        }

        SetNetworkListener();
        UIInitialize();
    }

    private void UIInitialize()
    {
        uiHandle.onClickedGameEntryButtonListener = GameEntry;
        uiHandle.onClickedExitButtonListener = ExitSession;
    }

    private void SetNetworkListener()
    {
        GameNetworkManager.Instance.AddJoinedUserEventListener(JoinedUserListener);

        GameNetworkManager.Instance.AddLeftUserEventListener(LeftUserListener);
    }

    private void GameEntry()
    {
        if (userStateHandler != null) userStateHandler.SetReadyState(!userStateHandler.readyState);
    }

    private async void ExitSession()
    {
        bool isExitRequest = true;

        Spinner.Instance.OnSpinner(() => isExitRequest == false);
        await GameNetworkManager.Instance.Connect();
        isExitRequest = false;

        SceneManager.LoadScene(SceneType.SceneType.Localinitialize.id, LoadSceneMode.Single);
    }

    private void JoinedUserListener(PlayerRef userRef)
    {
        WaitingRoomUserStateHandler.AddSpawnedCallback(userRef, RegistStateHandler);
    }

    private void LeftUserListener(PlayerRef userRef)
    {
        UnRegistStateHandler(userRef);
    }

    private void RegistStateHandler(WaitingRoomUserStateHandler handler)
    {
        if (handler.Object.StateAuthority == runner.LocalPlayer)
            userStateHandler = handler;
        else
            opponentStateHandler = handler;
    }

    private void UnRegistStateHandler(PlayerRef userRef)
    {
        opponentStateHandler = null;

        WaitingRoomUserStateHandler.RemoveSpawnedCallback(userRef, RegistStateHandler);
    }

    private void FixedUpdate()
    {
        if (userStateHandler != null)
        {
            userModel.SetActive(true);
            uiHandle.SetUserSlotActive(true);
            uiHandle.SetGameEntryButton(userStateHandler.readyState);
        }
        else
        {
            userModel.SetActive(false);
            uiHandle.SetUserSlotActive(false);
        }


        if (opponentStateHandler != null)
        {
            opponentModel.SetActive(true);
            uiHandle.SetOpponentSlotActive(true);
            uiHandle.SetOpponentReadyState(opponentStateHandler.readyState);
        }
        else
        {
            opponentModel.SetActive(false);
            uiHandle.SetOpponentSlotActive(false);
            uiHandle.SetOpponentReadyState(false);
        }

        CheckStartGame();
    }

    private void CheckStartGame()
    {
        if (runner.IsSharedModeMasterClient == false) return;

        bool fullUser = userStateHandler != null && opponentStateHandler != null;

        bool allReady =
            fullUser &&
            userStateHandler.readyState && opponentStateHandler.readyState;

        if (fullUser && allReady)
        {
            runner.LoadScene(NetScene.InGame.sceneRef, LoadSceneMode.Single);
            runner.SessionInfo.IsOpen = false;
        }
    }

    private void OnDestroy()
    {
        WaitingRoomUserStateHandler.ClearAll();
    }
}
