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
            if (SceneType.iMapBySceneName.TryGetValue(scene.name, out SceneType value) == false) return;

            currentSceneType = value;
            CheckCursorValidable();
        }

        private void CheckCursorValidable()
        {
            bool sceneSetting = currentSceneType == null ? true : currentSceneType.useCursor;
            bool uiOverlay = uiOverlayCount > 0;

            if (sceneSetting || uiOverlay)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OpenCursorUsableUI()
        {
            ++uiOverlayCount;
            CheckCursorValidable();
        }
        public void CloseCursorUsableUI()
        {
            --uiOverlayCount;
            CheckCursorValidable();
        }

    }
}

