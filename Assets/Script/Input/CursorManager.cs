using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Input
{
    using SceneType;
    using UnityEngine.SceneManagement;

    public class CursorManager : MonoSingleton<CursorManager>
    {
        private SceneType currentSceneType;

        private int uiOverlayCount = 0;

        protected override void OnAwake()
        {
            SceneManager.sceneLoaded += SceneLoadedListener;
        }

        private void SceneLoadedListener(Scene scene, LoadSceneMode mode)
        {
            
        }

    }
}

