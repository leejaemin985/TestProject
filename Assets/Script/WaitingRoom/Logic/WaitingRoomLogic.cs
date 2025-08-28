using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Fusion;

using SceneType;
using Utility.Spinner;
using WaitingRoom.Net;

namespace WaitingRoom.Logic
{
    using UI;

    public class WaitingRoomLogic : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [Header("Setting")]
        [SerializeField] private WaitingRoomUI uiHandle;

        [SerializeField] private GameObject userModel;
        [SerializeField] private GameObject opponentModel;

        [Header("Prefab")]
        [SerializeField] private WaitingRoomUserStateHandler stateHandlerPrefab;

        [Header("NetworkObject")]
        [SerializeField] private WaitingRoomUserStateHandler userStateHandler;
        [SerializeField] private WaitingRoomUserStateHandler opponentStateHandler;


        private async void Start()
        {
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
            if (userStateHandler != null)
            {
                userModel.SetActive(true);
                uiHandle.SetUserSlotActive(true);
                uiHandle.SetGameEntryButton(userStateHandler.readyState);
            }
            else
            {
                userModel.SetActive(false);
                uiHandle.SetUserSlotActive(false);
            }


            if (opponentStateHandler != null)
            {
                opponentModel.SetActive(true);
                uiHandle.SetOpponentSlotActive(true);
                uiHandle.SetOpponentReadyState(opponentStateHandler.readyState);
            }
            else
            {
                opponentModel.SetActive(false);
                uiHandle.SetOpponentSlotActive(false);
                uiHandle.SetOpponentReadyState(false);
            }

            CheckStartGame();
        }

        private bool test;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T)) test = true;
        }

        private void CheckStartGame()
        {
            if (runner.IsSharedModeMasterClient == false) return;

            bool fullUser = userStateHandler != null && opponentStateHandler != null;

            bool allReady =
                fullUser &&
                userStateHandler.readyState && opponentStateHandler.readyState;

            if (fullUser && allReady || test) 
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
