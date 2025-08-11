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

    private void Start()
    {
        runner.Spawn(
            waitingRoomUserHandlePrefab,
            Vector3.zero,
            Quaternion.identity,
            runner.LocalPlayer);

        UIInitialize();
        RefreshStatus();
        //GameNetworkManager.Instance.SetJoinedUserEventListener((userRef) => RefreshStatus());
        //GameNetworkManager.Instance.SetLeftUserEventListener((userRef) => RefreshStatus());
    }

    private void UIInitialize()
    {
        uiHandle.onClickedGameEntryButtonListener = GameEntry;
        uiHandle.onClickedExitButtonListener = ExitSession;
    }

    private void GameEntry()
    {
        var userHandle = GetLocalHandle();
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
        opponentModel.SetActive(GetOpponentHandle() != null);
    }

    private void RefreshStatus()
    {
        UpdateOpponentModelActive();
        CheckUsersReadyState();
    }

    private WaitingRoomUserHandle GetLocalHandle()
    {
        return userHandles.FirstOrDefault(x => x.Key == runner.LocalPlayer).Value;
    }

    private WaitingRoomUserHandle GetOpponentHandle()
    {
        return userHandles.FirstOrDefault(x => x.Key != runner.LocalPlayer).Value;
    }

    public void RegisterUserHandle(PlayerRef userRef, WaitingRoomUserHandle userHandle)
    {
        userHandles.Add(userRef, userHandle);
        userHandle.SetChangedReadyStateListener(CheckUsersReadyState);

        RefreshStatus();
    }

    public void UnregisterUserHandle(PlayerRef userRef, WaitingRoomUserHandle userHandle)
    {
        userHandles.Remove(userRef);

        RefreshStatus();
    }

    private void CheckUsersReadyState()
    {
        WaitingRoomUserHandle userHandle = GetLocalHandle();
        WaitingRoomUserHandle opponentHandle = GetOpponentHandle();

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

        var userHandle = GetLocalHandle();
        var opponentHandle = GetOpponentHandle();

        bool fullSession = userHandle != null && opponentHandle != null;

        bool allUsersReady = 
            fullSession &&
            userHandle.readyState && opponentHandle.readyState;

        bool isMaster = runner.IsSharedModeMasterClient;

        if (fullSession && allUsersReady && isMaster)
        {
            //SceneManager.LoadScene(SceneType.SceneType.InGame.id, LoadSceneMode.Single);
            runner.SceneManager.LoadScene(SceneRef.FromIndex(SceneType.SceneType.InGame.id), LoadSceneMode.Single);
        }

    }
}
