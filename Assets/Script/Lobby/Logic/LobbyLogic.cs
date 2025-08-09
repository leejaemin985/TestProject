using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Spinner;

namespace Lobby
{
    public class LobbyLogic : MonoBehaviour
    {
        [SerializeField] private LobbyUI uiHandle = default;
        [SerializeField] private LobbySessionScrollController sessionScrollController = default;

        private bool isEnteringSession = false;

        private void Start()
        {
            sessionScrollController.Initialize(TryEnterSession);
            GameNetworkManager.Instance.SetSessionUpdateEventListener((list) => sessionScrollController.UpdateSessionList(list));
            sessionScrollController.UpdateSessionList(GameNetworkManager.Instance.sessionList);

            UIInitialize();
        }

        private async void TryEnterSession(string sessionName)
        {
            isEnteringSession = true;
            Spinner.Instance.OnSpinner(() => isEnteringSession == false);
            await GameNetworkManager.Instance.runner.StartGame(new()
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                PlayerCount = 2
            });

            isEnteringSession = false;
            SceneManager.LoadScene(SceneType.SceneType.InGame.id, LoadSceneMode.Single);
        }

        private void UIInitialize()
        {
            SetUIListener();
        }
        private void SetUIListener()
        {
            uiHandle.onClickQuickStartListener = TryQuickStart;
            uiHandle.onClickMakeRoomPopupConfirmListener = TryMakeRoom;

            uiHandle.onClickMakeRoomPopupListener = () => SetMakeRoomPopup(true);
            uiHandle.onClickMakeRoomPopupCancelListener = () => SetMakeRoomPopup(false);
        }

        private bool CanEnterSession(SessionInfo sessionInfo)
        {
            return sessionInfo.PlayerCount < sessionInfo.MaxPlayers;
        }

        private string MakeSessionName(string customName)
        {
            Guid sessionUid = Guid.NewGuid();
            return $"{sessionUid}//{customName}";
        }

        private void TryQuickStart()
        {
            foreach (var session in GameNetworkManager.Instance.sessionList)
            {
                if (CanEnterSession(session))
                {
                    TryEnterSession(session.Name);
                    return;
                }
            }

            string sessionName = MakeSessionName("Test Session Name");
            TryEnterSession(sessionName);
        }

        private void TryMakeRoom()
        {
            string sessionName = MakeSessionName(uiHandle.GetSessionNameInputFieldText());
            TryEnterSession(sessionName);
        }

        private void SetMakeRoomPopup(bool set)
        {
            uiHandle.SetSessionNameInputFieldText(string.Empty);
            uiHandle.SetMakeRoomPopup(set);
        }


        #region Old


        //private void QuickStart()
        //{
        //    //if (cachedSessionInfo == null || cachedSessionInfo.Count == 0) return;
        //    //TryEntryRoom(cachedSessionInfo[0].Name);
        //}

        //private void SetMakeRoomPopup(bool set) => uiHandle.SetMakeRoomPopup(set);

        //private void TryEntryRoom(string sessionName)
        //{
        //    //SceneManager.LoadScene("Scenes/InGame", LoadSceneMode.Single);
        //    //uiHandle.gameObject.SetActive(false);
        //    //runner.StartGame(new()
        //    //{
        //    //    GameMode = GameMode.Shared,
        //    //    SessionName = sessionName,
        //    //    PlayerCount = 2
        //    //});
        //}

        #endregion
    }
}
