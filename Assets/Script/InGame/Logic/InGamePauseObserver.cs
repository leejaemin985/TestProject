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

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false) TryTransferMaster();
        }

        private void TryTransferMaster()
        {
            pauseEventListener?.Invoke();
        }

        private void OnDestroy()
        {
            pauseEventListener = null;
        }
    }
}