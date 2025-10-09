using System;
using System.Threading;

using UnityEngine;
using UnityEngine.SceneManagement;

using Fusion;

using SceneType;
using Utility.Spinner;

namespace WaitingRoom.Logic
{
    using Net;

    public class WaitingRoomLogic : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [Header("SubLogic")]
        [SerializeField] private WaitingRoomLocalSetting localSetting;

        [Header("Resources")]
        [SerializeField] private WaitingRoomUserStateHandler stateHandlerPrefab;

        [Header("NetworkObject")]
        [SerializeField] private WaitingRoomUserStateHandler userStateHandler;
        [SerializeField] private WaitingRoomUserStateHandler opponentStateHandler;

        private CancellationTokenSource cts;

        private async void Start()
        {
            if (runner.IsSharedModeMasterClient)
                runner.SessionInfo.IsOpen = true;

            cts = new();
            localSetting.InitializeAsync(GameEntry, ExitSession, cts.Token);

            var handle = await runner.SpawnAsync(prefab: stateHandlerPrefab);
            userStateHandler = handle.GetComponent<WaitingRoomUserStateHandler>();

            foreach (var user in GameNetworkManager.Instance.connectedUsers)
            {
                WaitingRoomUserStateHandler.AddSpawnedCallback(user, RegistStateHandler);
            }

            SetNetworkListener();
        }

        private void SetNetworkListener()
        {
            GameNetworkManager.Instance.AddJoinedUserEventListener(JoinedUserListener);
            GameNetworkManager.Instance.AddLeftUserEventListener(LeftUserListener);
        }

        private void GameEntry()
        {
            if (userStateHandler != null) userStateHandler.SetReadyState(!userStateHandler.readyState);
        }

        private async void ExitSession()
        {
            bool isExitRequest = true;
            cts?.Cancel();

            Spinner.Instance.OnSpinner(() => isExitRequest == false);
            await GameNetworkManager.Instance.Connect();
            
            isExitRequest = false;

            if (GameNetworkManager.Instance.isInitialized)
            {
                SceneManager.LoadScene(SceneType.SceneType.Lobby.id, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadScene(SceneType.SceneType.Localinitialize.id, LoadSceneMode.Single);
            }
        }

        private void JoinedUserListener(PlayerRef userRef)
        {
            WaitingRoomUserStateHandler.AddSpawnedCallback(userRef, RegistStateHandler);
        }

        private void LeftUserListener(PlayerRef userRef)
        {
            UnRegistStateHandler(userRef);
            WaitingRoomUserStateHandler.RemoveSpawnedCallback(userRef, RegistStateHandler);
        }

        private void RegistStateHandler(WaitingRoomUserStateHandler handler)
        {
            if (handler.Object.StateAuthority == runner.LocalPlayer)
                userStateHandler = handler;
            else
                opponentStateHandler = handler;
        }

        private void UnRegistStateHandler(PlayerRef userRef)
        {
            opponentStateHandler = null;
        }

        private void FixedUpdate()
        {
            localSetting.SetUserState(userStateHandler != null, userStateHandler ? userStateHandler.readyState : false);
            localSetting.SetOpponentState(opponentStateHandler != null, opponentStateHandler ? opponentStateHandler.readyState : false);
            CheckStartGame();
        }

        private void CheckStartGame()
        {
            bool fullUser = userStateHandler != null && opponentStateHandler != null;

            bool allReady =
                fullUser &&
                userStateHandler.readyState && opponentStateHandler.readyState;

            if (fullUser && allReady) 
            {
                Spinner.Instance.OnSpinner(() => false, true);

                if (runner.IsSharedModeMasterClient)
                {
                    runner.LoadScene(NetScene.InGame.sceneRef, LoadSceneMode.Single);
                    runner.SessionInfo.IsOpen = false;
                }
            }
        }

        private void OnDestroy()
        {
            GameNetworkManager.Instance.RemoveJoinedUserEventListener(JoinedUserListener);
            GameNetworkManager.Instance.RemoveLeftUserEventListener(LeftUserListener);
            WaitingRoomUserStateHandler.ClearAll();
        }
    }
}
