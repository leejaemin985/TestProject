using System;
using System.Collections;

using UnityEngine;

using Physics;
using Fusion;
using Fusion.Addons.SimpleKCC;

namespace Unit
{
    public class Player : Unit
    {
        [Header("Player")]
        [SerializeField] private PlayerFSM fsm;
        [SerializeField] private SimpleKCC cc;
        [SerializeField] private PlayerCam playerCam;
        [SerializeField] private Animator anim;

        [SerializeField] private PlayerAnimEventer animEventer;
        [SerializeField] private Katana weapon;

        [SerializeField] private Animator latencyInterpolatedAnim;
        [SerializeField] private Transform latencyInterpolatedWeapPos;

        private HitBox playerHitBox;

        private IEnumerator CamSettingHandle = default;
        
        public override void Spawned()
        {
            PlayerRegistry.Instance.RegisterPlayer(Object.StateAuthority, this);
            Initialize();

            UnitStat.AddSpawnedCallback(Object.StateAuthority, BindUnitStat);
        }

        protected override void Initialize()
        {
            base.Initialize();

            StartCamSet();
            
            if (Runner.IsSharedModeMasterClient)
                playerHitBox = InitPlayerHitBox();
            else
            {
                latencyInterpolatedAnim.gameObject.SetActive(false);
            }

            weapon.Initialize(Runner.IsSharedModeMasterClient, playerHitBox);
            weapon.SetCollisionPos(latencyInterpolatedWeapPos);
            
            fsm.Initialized(this, cc, anim, latencyInterpolatedAnim, weapon);
            animEventer.Initialize(fsm.AnimEvent);
        }

        public void RequestOnHitState(HitInfo hitInfo)
        {
            fsm?.OnHitState(hitInfo);
            OnDamaged(hitInfo.damaged);
        }

        public void RequestOnParringState(HitInfo hitInfo)
        {
            fsm?.OnParringState(hitInfo);
            OnDecreasePosture(hitInfo.damaged);
        }

        public void RequestOnDiedState(HitInfo hitInfo)
        {
            fsm?.OnDiedState(hitInfo);
            OnDamaged(hitInfo.damaged);

            playerHitBox.SetActive(false);
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

                default: // died
                    EventDispatcher.Instance.RequestOnDiedUser(Object.InputAuthority, hitInfo);
                    break;
            }
        }

        #region Cam
        private void StartCamSet()
        {
            if (HasInputAuthority == false) return;

            if (CamSettingHandle != null) StopCoroutine(CamSettingHandle);
            StartCoroutine(CamSettingHandle = CamSetting());

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private IEnumerator CamSetting()
        {
            yield return new WaitUntil(() => Runner.IsRunning);

            playerCam.SetCam();
        }
        #endregion

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
