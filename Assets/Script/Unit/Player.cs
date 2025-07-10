using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unit;
using UnityEngine;

namespace Unit
{
    public class Player : Unit
    {
        private PlayerRef cachedPlayerRef;

        [Networked] 
        public PlayerRef userRef { get; private set; }

        [Networked]
        public Vector3 moveAnimDir { get; set; }

        [Networked]
        public float runWeight { get; set; }


        [SerializeField] private SimpleKCC cc;
        [SerializeField] private PlayerCam playerCam;
        [SerializeField] private Animator anim;

        [SerializeField] private PlayerAnimEventer animEventer;
        [SerializeField] private Katana weapon;

        private PlayerFSM fsm;

        private IEnumerator CamSettingHandle = default;

        public void PreSpawnInitialize(PlayerRef userRef)
        {
            this.cachedPlayerRef = userRef;
        }

        public override void Spawned()
        {
            if (HasStateAuthority) userRef = cachedPlayerRef;

            if (initSequencerHandle != null) StopCoroutine(initSequencerHandle);
            StartCoroutine(initSequencerHandle = InitSequencer());
        }

        private IEnumerator initSequencerHandle = null;

        private IEnumerator InitSequencer()
        {
            yield return new WaitUntil(() => GameManager.Instance.isInitialized);

            PlayerRegistry.Instance.RegisterPlayer(userRef, this);
            Initialize();
        }

        private void Initialize()
        {
            StartCamSet();
            fsm = Instantiate(Resources.Load<PlayerFSM>(PlayerFSM.RESOURCES_PATH));
            fsm.transform.SetParent(transform, false);
            fsm.Initialized(this, cc, anim);

        }

        public override void Render()
        {
            if (!HasInputAuthority)
            {
                Debug.Log($"{moveAnimDir}");
            }

            anim.SetFloat("_Horizontal", moveAnimDir.x);
            anim.SetFloat("_Vertical", moveAnimDir.z);
            anim.SetFloat("_RunWeight", runWeight);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput<InputData>(out var input) == false) return;

            fsm?.UpdateTick(input, Runner.Tick, Runner.DeltaTime);
        }

        #region Cam
        private void StartCamSet()
        {
            if (CamSettingHandle != null) StopCoroutine(CamSettingHandle);
            StartCoroutine(CamSettingHandle = CamSetting());
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private IEnumerator CamSetting()
        {
            if (HasInputAuthority == false) yield break;
            yield return new WaitUntil(() => Runner.IsRunning);

            playerCam.SetCam();
        }
        #endregion
    }
}
