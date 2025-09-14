using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class InGameCamManager : MonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook freeLookCam;
        [SerializeField] private Transform lookTarget;

        private Camera cam;

        public void Initialize(Camera cam, Transform lookTarget)
        {
            this.cam = cam;
            //this.lookTarget = lookTarget;

            freeLookCam.enabled = true;
            freeLookCam.LookAt = this.lookTarget;//lookTarget;
            freeLookCam.Follow = this.lookTarget;//lookTarget;

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
