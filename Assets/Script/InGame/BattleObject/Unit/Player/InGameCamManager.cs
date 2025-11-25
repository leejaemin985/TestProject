using System;
using UnityEngine;
using Cinemachine;
using Unit;

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
        [SerializeField] private NoiseSettings noiseProfile;

        private Transform noneStateTransform;
        private Transform motionStateTransform;

        public void Initialize(Camera cam,Transform player,Transform noneStateTransform, Transform motionStateTransform)
        {
            freeLookCam.enabled = true;
            freeLookCam.Follow = player;

            transform.SetParent(null);

            this.noneStateTransform = noneStateTransform;
            this.motionStateTransform = motionStateTransform;
            SetCamState(CamActionState.None);
        }

        private void SetCamState(CamActionState state)
        {
            freeLookCam.LookAt = true || state == CamActionState.None ? noneStateTransform : motionStateTransform;
        }

        public void ChangeUserStateListener(PlayerStateBase.StateType stateType)
        {
            switch (stateType)
            {
                case PlayerStateBase.StateType.Hit:
                case PlayerStateBase.StateType.Parring:
                    SetCamState(CamActionState.Motion);
                    break;

                default:
                    SetCamState(CamActionState.None);
                    break;
            }
        }

    }
}
