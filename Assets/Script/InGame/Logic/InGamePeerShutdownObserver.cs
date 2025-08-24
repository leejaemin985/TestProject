using System;
using UnityEngine;
using Fusion;
using SceneType;
using System.Collections;

namespace InGame.Logic
{
    public class InGamePeerShutdownObserver : MonoBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        private IEnumerator exitInGameHandle;

        private void Awake()
        {
            GameNetworkManager.Instance.AddLeftUserEventListener(UserLeftSession);
        }

        private void UserLeftSession(PlayerRef userRef)
        {
            if (userRef == runner.LocalPlayer) return;

            if (exitInGameHandle != null) StopCoroutine(exitInGameHandle);
            StartCoroutine(exitInGameHandle = exitSessionRoutine());
        }

        private IEnumerator exitSessionRoutine()
        {
            yield return new WaitUntil(() => runner.IsSharedModeMasterClient);
            runner.LoadScene(NetScene.WaitingRoom.sceneRef, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            GameNetworkManager.Instance.RemoveLeftUserEventListener(UserLeftSession);
        }
    }
}