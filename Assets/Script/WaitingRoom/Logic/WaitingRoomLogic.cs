using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitingRoomLogic : MonoBehaviour
{
    [SerializeField] private WaitingRoomUI uiHandle = default;

    [SerializeField] private GameObject userModel = default;
    [SerializeField] private GameObject opponentModel = default;


    private void Start()
    {
        UIInitialize();

    }

    private void UIInitialize()
    {
        uiHandle.Initialize();
        uiHandle.onClickedGameEntryButtonListener = GameEntry;
        uiHandle.onClickedExitButtonListener = ExitSession;
    }

    private void GameEntry()
    {
        Debug.Log($"Test - GameEntry");
    }

    private async void ExitSession()
    {
        await GameNetworkManager.Instance.runner.Shutdown();
        SceneManager.LoadScene(SceneType.SceneType.Localinitialize.id, LoadSceneMode.Single);
    }
}
