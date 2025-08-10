using Fusion;
using System;

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
            GameNetworkManager.Instance.SetSessionUpdateEventListener((list) => sessionScrollController.UpdateSessionList(list));
            sessionScrollController.UpdateSessionList(GameNetworkManager.Instance.sessionList);

            UIInitialize();
        }

        private async void EnterSession(string sessionName)
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
