using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame
{
    public class InGameCamManager : MonoBehaviour
    {
        public enum CamActionState
        {
            None,
            Motion
        }

        [SerializeField] private CinemachineFreeLook freeLookCam;
        
        private Transform noneStateTransform;
        private Transform motionStateTransform;

        public void Initialize(Camera cam, Transform noneStateTransform, Transform motionStateTransform)
        {
            freeLookCam.enabled = true;
            this.noneStateTransform = noneStateTransform;
            this.motionStateTransform = motionStateTransform;
            SetCamState(CamActionState.None);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SetCamState(CamActionState state)
        {
            Transform targetPos = state == CamActionState.None ? noneStateTransform : motionStateTransform;
            freeLookCam.LookAt = targetPos;
            freeLookCam.Follow = targetPos;
        }

        /// Entry Point에서 락 해제 중
        //private void OnDestroy()
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //}
    }
}
