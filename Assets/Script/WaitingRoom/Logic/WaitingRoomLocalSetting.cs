using Addressable;
using Cinemachine;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utility.Spinner;
using WaitingRoom.UI;

namespace WaitingRoom
{
    public class WaitingRoomLocalSetting : MonoBehaviour
    {
        NetworkRunner runner => GameNetworkManager.Instance.runner;

        [Header("Setting")]
        [SerializeField] private WaitingRoomUI uiHandle;

        [SerializeField] private Transform userModelPos;
        [SerializeField] private Transform opponentModelPos;

        [SerializeField] private CinemachineVirtualCamera waitLoadingVirtualCam;
        [SerializeField] private CinemachineVirtualCamera mainVirtualCam;


        [Header("Resources")]
        [SerializeField] private RuntimeAnimatorController modelAnimController;


        private GameObject userModel;
        private GameObject opponentModel;

        private Action onGameEnteryEvent;
        private Action onExitSessionEvent;

        public async void InitializeAsync(Action onGameEntryAction, Action onExitSessionAction, CancellationToken cancellationToken)
        {
            try
            {
                this.onGameEnteryEvent = onGameEntryAction;
                this.onExitSessionEvent = onExitSessionAction;
                UIInitialize();

                SetVirtualCamPos(false);

                await LoadModel(cancellationToken);

                SetVirtualCamPos(true);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return;
            }
        }

        private void UIInitialize()
        {
            uiHandle.SetSessionInfo(runner.SessionInfo);
            uiHandle.onClickedGameEntryButtonListener = onGameEnteryEvent;
            uiHandle.onClickedExitButtonListener = onExitSessionEvent;
        }

        private async Task LoadModel(CancellationToken cancellationToken)
        {
            GameObject samuraiModel = await AddressableManager.LoadAsst<GameObject>(AddressableKey.PK_SamuraiModel);
            
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                userModel = Instantiate(samuraiModel);
                userModel.GetComponent<Animator>().runtimeAnimatorController = modelAnimController;
                userModel.transform.SetPositionAndRotation(userModelPos.position, userModelPos.rotation);

                await Task.Yield();

                cancellationToken.ThrowIfCancellationRequested();

                opponentModel = Instantiate(samuraiModel);
                opponentModel.GetComponent<Animator>().runtimeAnimatorController = modelAnimController;
                opponentModel.transform.SetPositionAndRotation(opponentModelPos.position, opponentModelPos.rotation);
            }
            catch(OperationCanceledException)
            {
                CleanupModels();
                Debug.LogWarning("OperationCanceledException - Cancel Loading Model in WaitingRoom");
            }
            catch(Exception e)
            {
                Debug.LogError($"WaitingRoom - {e}");
            }
            finally
            {

            }
        }

        public void SetUserState(bool spawnedUser, bool readyState)
        {
            if (userModel == null) return;
            userModel.SetActive(spawnedUser);
            uiHandle.SetUserSlotActive(spawnedUser);

            uiHandle.SetGameEntryButton(readyState);
        }

        public void SetOpponentState(bool spawnedOpponent, bool readyState)
        {
            if (opponentModel == null) return;
            opponentModel.SetActive(spawnedOpponent);
            uiHandle.SetOpponentSlotActive(spawnedOpponent);

            uiHandle.SetOpponentReadyState(readyState);
        }

        private void CleanupModels()
        {
            if (userModel != null)
            {
                Destroy(userModel);
                userModel = null;
            }
            if (opponentModel != null)
            {
                Destroy(opponentModel);
                opponentModel = null;
            }
            AddressableManager.Release(AddressableKey.PK_SamuraiModel);
        }

        private void SetVirtualCamPos(bool loadedLocalSetting)
        {
            waitLoadingVirtualCam.Priority = loadedLocalSetting ? 0 : 1;
            mainVirtualCam.Priority = loadedLocalSetting ? 1 : 0;
        }

        private void OnDestroy()
        {
            CleanupModels();
        }
    }
}
