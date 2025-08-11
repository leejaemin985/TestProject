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

    private void Start()
    {
        GameNetworkManager.Instance.runner.Spawn(
            waitingRoomUserHandlePrefab,
            Vector3.zero,
            Quaternion.identity,
            GameNetworkManager.Instance.runner.LocalPlayer);

        RefreshStatus();
        GameNetworkManager.Instance.SetJoinedUserEventListener((userRef) => RefreshStatus());
        GameNetworkManager.Instance.SetLeftUserEventListener((userRef) => RefreshStatus());
    }

    private void UIInitialize()
    {
        uiHandle.Initialize();
        uiHandle.onClickedGameEntryButtonListener = GameEntry;
        uiHandle.onClickedExitButtonListener = ExitSession;
    }

    private void GameEntry()
    {
        userHandles[GameNetworkManager.Instance.runner.LocalPlayer].ChangedReadyState();
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
        opponentModel.SetActive(GameNetworkManager.Instance.connectedUsers.Count > 1);
    }

    private void RefreshStatus()
    {
        UIInitialize();
        UpdateOpponentModelActive();
    }

    public void RegisterUserHandle(PlayerRef userRef, WaitingRoomUserHandle userHandle)
    {
        userHandles.Add(userRef, userHandle);
        userHandle.SetChangedReadyStateListener(CheckUsersReadyState);
    }

    public void UnregisterUserHandle(PlayerRef userRef, WaitingRoomUserHandle userHandle)
    {
        userHandles.Remove(userRef);
    }

    private void CheckUsersReadyState()
    {
        if (!GameNetworkManager.Instance.runner.IsSharedModeMasterClient) return;

        var opponentHandle = userHandles
            .Where(kv => kv.Key != GameNetworkManager.Instance.runner.LocalPlayer)
            .Select(kv => kv.Value)
            .FirstOrDefault();

        if (opponentHandle != null)
        {
            uiHandle.SetOpponentReadyCheck(opponentHandle.readyState);
        }

        bool fullSession = userHandles.Count == 2;
        bool allReadyState = true;
        foreach (var userHandle in userHandles)
        {
            if (!userHandle.Value.readyState) allReadyState = false;
        }

        if (fullSession && allReadyState)
        {
            Debug.Log($"Test - StartGame!");
        }
    }
}
