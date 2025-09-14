using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class InGameCamManager : MonoBehaviour
    {
        private Camera cam;
        private CinemachineFreeLook freeLookCam;
        private Transform lookTarget;

        public void Initialize(Camera cam, Transform lookTarget)
        {
            this.cam = cam;
            this.lookTarget = lookTarget;

            freeLookCam = gameObject.AddComponent<CinemachineFreeLook>();
            freeLookCam.LookAt = lookTarget;
            freeLookCam.Follow = lookTarget;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        /// Entry Point에서 락 해제 중
        //private void OnDestroy()
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //}
    }
}
