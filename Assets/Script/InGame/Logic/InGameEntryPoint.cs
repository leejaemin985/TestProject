using Fusion;
using InGame.Logic.Flow;
using SceneType;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Logic
{
    public class InGameEntryPoint : SimulationBehaviour
    {
        private NetworkRunner runner => GameNetworkManager.Instance.runner;

        [SerializeField] private PhaseSequencer phaseSequencerPrefab;

        private IEnumerator exitSessionHandle;

        public async void Start()
        {
            if (runner.IsSharedModeMasterClient)
            {
                await runner.SpawnAsync(prefab: phaseSequencerPrefab);
            }

            SetNetListener();
        }

        private void SetNetListener()
        {
            GameNetworkManager.Instance.AddLeftUserEventListener(UserLeftSession);
        }

        private void UserLeftSession(PlayerRef userRef)
        {
            if (userRef == runner.LocalPlayer) return;

            if (exitSessionHandle != null) StopCoroutine(exitSessionHandle);
            StartCoroutine(exitSessionHandle=exitSessionRoutine());
        }

        private IEnumerator exitSessionRoutine()
        {
            yield return new WaitUntil(() => runner.IsSharedModeMasterClient);
            runner.LoadScene(NetScene.WaitingRoom.sceneRef, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            GameNetworkManager.Instance.RemoveLeftUserEventListener(UserLeftSession);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
