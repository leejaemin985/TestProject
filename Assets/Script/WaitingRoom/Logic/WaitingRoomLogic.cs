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

    [SerializeField] private WaitingRoomUserHandle userHandlePrefab = default;

    private void Start()
    {
        GameNetworkManager.Instance.runner.Spawn(userHandlePrefab, Vector3.zero, Quaternion.identity, GameNetworkManager.Instance.runner.LocalPlayer);

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
        //Debug.Log($"Test - GameEntry");
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
}
