using Fusion;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Logic
{
    public class InGamePauseObserver : MonoBehaviour
    {
        private static NetworkRunner runner => GameNetworkManager.Instance.runner;

        private static Action pauseEventListener;
        
        public static void AddPauseEventListener(Action eventListener)
        {
            pauseEventListener -= eventListener;
            pauseEventListener += eventListener;
        }

        public static void RemovePauseEventListener(Action eventListener)
        {
            pauseEventListener -= eventListener;
        }

        //private void OnApplicationPause(bool pause)
        //{

        //}

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false) TryTransferMasterAsync();
        }

        private async void TryTransferMasterAsync()
        {
            pauseEventListener?.Invoke();
        }

        private void OnDestroy()
        {
            pauseEventListener = null;
        }
    }
}