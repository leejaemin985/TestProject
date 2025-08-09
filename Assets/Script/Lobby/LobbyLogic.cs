using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyLogic : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    private NetworkRunner runner;

    [SerializeField] private LobbyUI uiHandle;

    private List<SessionInfo> cachedSessionInfo;

    private async void Start()
    {
        runner = Instantiate(runnerPrefab);
        var sessionReceiver = runner.GetComponent<SessionListReceiver>();
        sessionReceiver.SetSessionUpdateListener(UpdateSessionList);

        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer, null);
        
        UIInitialize();
    }

    private void UpdateSessionList(List<SessionInfo> sessionList)
    {
        cachedSessionInfo = sessionList;
    }

    private void UIInitialize()
    {
        SetUIListener();
    }

    private void SetUIListener()
    {
        uiHandle.onClickQuickStartListener = QuickStart;
        uiHandle.onClickMakeRoomConfirmListener = () => TryEntryRoom(uiHandle.GetSessionNameInputFieldText());

        uiHandle.onClickMakeRoomListener = () => SetMakeRoomPopup(true);
        uiHandle.onClickMakeRoomCancelListener = () => SetMakeRoomPopup(false);
    }

    private void QuickStart()
    {
        if (cachedSessionInfo == null || cachedSessionInfo.Count == 0) return;
        TryEntryRoom(cachedSessionInfo[0].Name);
    }

    private void SetMakeRoomPopup(bool set) => uiHandle.SetMakeRoomPopup(set);

    private void TryEntryRoom(string sessionName)
    {
        SceneManager.LoadScene("Scenes/InGame", LoadSceneMode.Single);
        //uiHandle.gameObject.SetActive(false);
        //runner.StartGame(new()
        //{
        //    GameMode = GameMode.Shared,
        //    SessionName = sessionName,
        //    PlayerCount = 2
        //});
    }

}
