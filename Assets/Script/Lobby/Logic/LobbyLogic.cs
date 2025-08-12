using Fusion;
using System;
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
            sessionScrollController.Initialize(TryJoinSession);
            sessionScrollController.UpdateSessionList(GameNetworkManager.Instance.sessionList);
            GameNetworkManager.Instance.SetSessionUpdateEventListener((list) => sessionScrollController.UpdateSessionList(list));

            UIInitialize();
        }

        private async void EnterSession(string sessionName)
        {
            isEnteringSession = true;
            Spinner.Instance.OnSpinner(() => isEnteringSession == false);

            var customprop = new Dictionary<string, SessionProperty>()
            {
                { "Started", false }
            };

            var joinResult = await GameNetworkManager.Instance.runner.StartGame(new()
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                PlayerCount = 2,
                SessionProperties = customprop
            });

            isEnteringSession = false;

            if (joinResult.Ok == false)
            {
                Debug.Log($"Lobby Logic - Failed Join Session (SessionName: {sessionName})");
                return;
            }

            SceneManager.LoadScene(SceneType.SceneType.WaitingRoom.id, LoadSceneMode.Single);
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

        private string MakeNewSessionName(string customName)
        {
            Guid sessionUid = Guid.NewGuid();
            return $"{sessionUid}//{customName}";
        }

        private void TryJoinSession(SessionInfo info)
        {
            if (GameNetworkManager.Instance.CanEnterSession(info) == false) return;
            EnterSession(info.Name);
        }

        private void TryQuickStart()
        {
            foreach (var session in GameNetworkManager.Instance.sessionList)
            {
                if (GameNetworkManager.Instance.CanEnterSession(session))
                {
                    EnterSession(session.Name);
                    return;
                }
            }

            string sessionName = MakeNewSessionName("New Session");
            EnterSession(sessionName);
        }

        private void TryMakeRoom()
        {
            string sessionName = MakeNewSessionName(uiHandle.GetSessionNameInputFieldText());
            EnterSession(sessionName);
        }


        private void SetMakeRoomPopup(bool set)
        {
            uiHandle.SetSessionNameInputFieldText(string.Empty);
            uiHandle.SetMakeRoomPopup(set);
        }
    }
}
