using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using UnityEngine;
using Physics;

namespace Unit
{
    public class Player : Unit
    {
        [SerializeField] private PlayerFSM fsm;
        [SerializeField] private SimpleKCC cc;
        [SerializeField] private PlayerCam playerCam;
        [SerializeField] private Animator anim;

        [SerializeField] private PlayerAnimEventer animEventer;
        [SerializeField] private Katana weapon;

        private HitBox playerHitBox;

        private IEnumerator CamSettingHandle = default;

        public override void Spawned()
        {
            if (initSequencerHandle != null) StopCoroutine(initSequencerHandle);
            StartCoroutine(initSequencerHandle = InitSequencer());
        }

        private IEnumerator initSequencerHandle = null;

        private IEnumerator InitSequencer()
        {
            yield return new WaitUntil(() => GameManager.Instance.isInitialized);
            
            PlayerRegistry.Instance.RegisterPlayer(Object.InputAuthority, this);
            Initialize();
        }

        private void Initialize()
        {
            StartCamSet();
            
            if (Runner.IsSharedModeMasterClient)
                playerHitBox = InitPlayerHitBox();

            weapon.Initialize(Runner.IsSharedModeMasterClient, playerHitBox);
            
            fsm.Initialized(this, cc, anim, weapon);
            animEventer.Initialize(fsm.AnimEvent);
        }

        public void RequestOnHitState(HitInfo hitInfo)
        {
            fsm?.OnHitState(hitInfo);
        }

        public void RequestOnParringState(HitInfo hitInfo)
        {
            fsm?.OnParringState(hitInfo);
        }

        private HitBox InitPlayerHitBox()
        {
            var ret = new GameObject("PlayerHitBox").AddComponent<HitBox>();
            ret.physicsShapeType = PhysicsObject.PhysicsShapeType.CAPSULE;

            ret.transform.SetParent(transform);
            ret.transform.localPosition = new Vector3(0, 1, 0);
            ret.transform.localScale = new Vector3(.7f, 2, .7f);

            ret.Initialize(HitEvent);
            ret.SetActive(true);

            return ret;
        }

        private void HitEvent(HitInfo hitInfo)
        {
            PlayerFSM.HitResultType result = fsm.CheckHittable(hitInfo);
            switch (result)
            {
                case PlayerFSM.HitResultType.Hit:
                    EventDispatcher.Instance.RequestOnHitUser(Object.InputAuthority, hitInfo);
                    break;

                case PlayerFSM.HitResultType.Parry:
                    EventDispatcher.Instance.RequestOnParringUser(Object.InputAuthority, hitInfo);
                    break;
            }
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
