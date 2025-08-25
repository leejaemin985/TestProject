using System;
using System.Collections.Generic;

using UnityEngine;

using Fusion;

using SceneType;
using Utility.Spinner;

namespace Lobby.Logic
{
    using UI;

    public class LobbyLogic : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [SerializeField] private LobbyUI uiHandle;
        [SerializeField] private LobbySessionScrollController sessionScrollController;

        private bool isEnteringSession = false;

        private void Start()
        {
            sessionScrollController.Initialize(TryJoinSession);

            sessionScrollController.UpdateSessionList(GameNetworkManager.Instance.sessionList);
            GameNetworkManager.Instance.AddSessionUpdateEventListener(UpdateSessionList);

            SetUIListener();
        }

        private void UpdateSessionList(List<SessionInfo> sessions)
        {
            sessionScrollController.UpdateSessionList(sessions);
        }

        private void SetUIListener()
        {
            uiHandle.onClickQuickStartListener = TryQuickStart;
            uiHandle.onClickMakeRoomPopupConfirmListener = TryMakeRoom;

            uiHandle.onClickMakeRoomPopupListener = () => SetMakeRoomPopup(true);
            uiHandle.onClickMakeRoomPopupCancelListener = () => SetMakeRoomPopup(false);
        }


        private string MakeNewSessionName(string customName) => $"{SessionMetaReader.GetNewSessionGuid()}{customName}";

        private async void EnterSession(string sessionName)
        {
            try
            {
                isEnteringSession = true;
                Spinner.Instance.OnSpinner(() => isEnteringSession == false);

                //MasterClient가 아닐때 scene세팅이 무시됩니다.
                NetworkSceneInfo sceneInfo = new();
                sceneInfo.AddSceneRef(NetScene.WaitingRoom.sceneRef);

                var joinResult = await runner.StartGame(new()
                {
                    GameMode = GameMode.Shared,
                    SessionName = sessionName,
                    PlayerCount = 2,
                    Scene = sceneInfo
                });

                
                if (joinResult.Ok == false)
                {
                    Debug.LogError($"{joinResult.ErrorMessage}");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isEnteringSession = false;
            }
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

        private void OnDestroy()
        {
            GameNetworkManager.Instance.RemoveSessionUpdateEventListener(UpdateSessionList);
        }
    }
}
