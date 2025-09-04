using System;

using UnityEngine;
using UnityEngine.SceneManagement;

using Addressable;
using Fusion;

using SceneType;
using Utility.Spinner;

namespace WaitingRoom.Logic
{
    using UI;
    using Net;
    using System.Threading.Tasks;
    using UnityEngine.AddressableAssets;

    public class WaitingRoomLogic : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [Header("Setting")]
        [SerializeField] private WaitingRoomUI uiHandle;

        [Header("Resources")]
        [SerializeField] private RuntimeAnimatorController modelAnimController;
        [SerializeField] private WaitingRoomUserStateHandler stateHandlerPrefab;

        [Header("NetworkObject")]
        [SerializeField] private WaitingRoomUserStateHandler userStateHandler;
        [SerializeField] private WaitingRoomUserStateHandler opponentStateHandler;

        [SerializeField] private GameObject userModel;
        [SerializeField] private GameObject opponentModel;

        private async void Start()
        {
            LoadWaitingRoomModels();

            if (runner.IsSharedModeMasterClient)
                runner.SessionInfo.IsOpen = true;

            var handle = await runner.SpawnAsync(prefab: stateHandlerPrefab);
            userStateHandler = handle.GetComponent<WaitingRoomUserStateHandler>();

            foreach (var user in GameNetworkManager.Instance.connectedUsers)
            {
                WaitingRoomUserStateHandler.AddSpawnedCallback(user, RegistStateHandler);
            }

            SetNetworkListener();
            UIInitialize();

        }

        private void SetNetworkListener()
        {
            GameNetworkManager.Instance.AddJoinedUserEventListener(JoinedUserListener);
            GameNetworkManager.Instance.AddLeftUserEventListener(LeftUserListener);
        }

        private void UIInitialize()
        {
            uiHandle.SetSessionInfo(runner.SessionInfo);
            uiHandle.onClickedGameEntryButtonListener = GameEntry;
            uiHandle.onClickedExitButtonListener = ExitSession;
        }

        private async void LoadWaitingRoomModels()
        {
            try
            {
                GameObject samuraiModel = await AddressableManager.LoadAsst<GameObject>(AddressableKey.PK_SamuraiModel);

                var userTask = Addressables.InstantiateAsync(AddressableKey.PK_SamuraiModel.key);

                var userModelTask = Task.Run(async () =>
                {
                    var model = await Addressables.InstantiateAsync(AddressableKey.PK_SamuraiModel).Task;
                    userModel = model;
                });

                var opponentModelTask = Task.Run(async () =>
                {
                    var model = await Addressables.InstantiateAsync(AddressableKey.PK_SamuraiModel).Task;
                    opponentModel = model;
                });

                await Task.WhenAll(userModelTask, opponentModelTask);

            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return;
            }
        }

        private void GameEntry()
        {
            if (userStateHandler != null) userStateHandler.SetReadyState(!userStateHandler.readyState);
        }

        private async void ExitSession()
        {
            bool isExitRequest = true;

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
            if (userModel != null)
            {
                userModel.SetActive(userStateHandler != null);
                uiHandle.SetUserSlotActive(userStateHandler != null);

                if (userStateHandler != null) uiHandle.SetGameEntryButton(userStateHandler.readyState);
            }

            if (opponentModel != null)
            {
                opponentModel.SetActive(opponentStateHandler != null);
                uiHandle.SetOpponentSlotActive(opponentStateHandler != null);

                if (opponentStateHandler != null) uiHandle.SetOpponentReadyState(opponentStateHandler.readyState);
            }
            CheckStartGame();
        }

        private void CheckStartGame()
        {
            if (runner.IsSharedModeMasterClient == false) return;

            bool fullUser = userStateHandler != null && opponentStateHandler != null;

            bool allReady =
                fullUser &&
                userStateHandler.readyState && opponentStateHandler.readyState;

            if (fullUser && allReady) 
            {
                runner.LoadScene(NetScene.InGame.sceneRef, LoadSceneMode.Single);
                runner.SessionInfo.IsOpen = false;
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
