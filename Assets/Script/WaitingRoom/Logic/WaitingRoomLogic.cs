using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Spinner;

public class WaitingRoomLogic : MonoBehaviour
{
    [SerializeField] private WaitingRoomUI uiHandle = default;

    [SerializeField] private GameObject userModel = default;
    [SerializeField] private GameObject opponentModel = default;

    [SerializeField] private WaitingRoomUserHandle waitingRoomUserHandlePrefab = default;

    private Dictionary<PlayerRef, WaitingRoomUserHandle> userHandles = new();

    private NetworkRunner runner => GameNetworkManager.Instance.runner;

    private WaitingRoomUserHandle userHandle = null;
    private WaitingRoomUserHandle opponentHandle = null;

    private void Start()
    {
        runner.SpawnAsync(
            waitingRoomUserHandlePrefab,
            Vector3.zero,
            Quaternion.identity,
            runner.LocalPlayer);

        UIInitialize();
        RefreshStatus();
    }

    private void UIInitialize()
    {
        uiHandle.onClickedGameEntryButtonListener = GameEntry;
        uiHandle.onClickedExitButtonListener = ExitSession;
    }

    private void GameEntry()
    {
        if (userHandle != null) userHandle.ChangedReadyState();
    }

    private async void ExitSession()
    {
        bool isExitRequest = true;
        Spinner.Instance.OnSpinner(() => isExitRequest == false);
        await GameNetworkManager.Instance.runner.Shutdown();
        isExitRequest = false;

        SceneManager.LoadScene(SceneType.SceneType.Localinitialize.id, LoadSceneMode.Single);
    }

    private void UpdateOpponentModelActive()
    {
        opponentModel.SetActive(opponentHandle != null);
    }

    private void RefreshStatus()
    {
        UpdateOpponentModelActive();
        CheckUsersReadyState();
    }

    public void RegisterUserHandle(PlayerRef userRef, WaitingRoomUserHandle userHandle)
    {
        userHandles.Add(userRef, userHandle);
        userHandle.SetChangedReadyStateListener(CheckUsersReadyState);

        if (userRef == runner.LocalPlayer) this.userHandle = userHandle;
        else opponentHandle = userHandle;

        RefreshStatus();
    }

    public void UnregisterUserHandle(PlayerRef userRef, WaitingRoomUserHandle userHandle)
    {
        userHandles.Remove(userRef);

        if (userRef == runner.LocalPlayer) this.userHandle = null;
        else opponentHandle = null;

        RefreshStatus();
    }

    private void CheckUsersReadyState()
    {
        if (userHandle != null)
            uiHandle.SetGameEntryButton(userHandle.readyState);

        if (opponentHandle != null)
        {
            uiHandle.SetOpponentReadyCheck(opponentHandle.readyState);
            uiHandle.SetOpponentSlotActive(true);
        }
        else
        {
            uiHandle.SetOpponentSlotActive(false);
        }

        CheckStartGame();
    }

    private void CheckStartGame()
    {
        if (!runner.IsSharedModeMasterClient) return;

        bool fullSession = userHandle != null && opponentHandle != null;

        bool allUsersReady =
            fullSession &&
            userHandle.readyState && opponentHandle.readyState;

        bool isMaster = runner.IsSharedModeMasterClient;

        if (fullSession && allUsersReady && isMaster)
        {
            runner.LoadScene(SceneRef.FromIndex(SceneType.SceneType.InGame.id), LoadSceneMode.Single);
            runner.SessionInfo.IsOpen = false;
        }
    }
}
