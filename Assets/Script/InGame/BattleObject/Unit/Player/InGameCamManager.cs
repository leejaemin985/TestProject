using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame
{
    public class InGameCamManager : MonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook freeLookCam;
        [SerializeField] private Transform lookTarget;

        //[SerializeField] private float speed = .2f;
        //private Transform head;

        private Camera cam;

        public void Initialize(Camera cam, Transform lookTarget)
        {
            this.cam = cam;
            //this.lookTarget = lookTarget;
            //head = lookTarget;
            //Debug.Log($"Test - head: {lookTarget}");

            freeLookCam.enabled = true;
            freeLookCam.LookAt = this.lookTarget;
            freeLookCam.Follow = this.lookTarget;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ShakeTest(float value)
        {
            freeLookCam.AddComponent<CinemachineBasicMultiChannelPerlin>();

        }

        //private void Update()
        //{
        //    if (head == null) return;
        //    lookTarget.position = Vector3.Lerp(lookTarget.position, head.position, speed);
            
        //}


        /// Entry Point에서 락 해제 중
        //private void OnDestroy()
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //}
    }
}
