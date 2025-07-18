using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using UnityEngine;
using Physics;
using System.Linq;

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
        private HitBox playerHitBox;
        private readonly Vector3 hitBoxLocalPos = new Vector3(0, 1, 0);
        private readonly Vector3 hitBoxScale = new Vector3(.7f, 2, .7f);

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

            if (Runner.IsSharedModeMasterClient)
            {
                playerHitBox = new GameObject("PlayerHitBox").AddComponent<HitBox>();
                playerHitBox.physicsShapeType = PhysicsObject.PhysicsShapeType.CAPSULE;
                playerHitBox.transform.SetParent(transform);
                playerHitBox.transform.localPosition = hitBoxLocalPos;
                playerHitBox.transform.localScale = hitBoxScale;


                playerHitBox.Initialize(HitEvent);
                playerHitBox.SetActive(true);
            }

            weapon.Initialize(Runner.IsSharedModeMasterClient, playerHitBox);

            fsm = Instantiate(Resources.Load<PlayerFSM>(PlayerFSM.RESOURCES_PATH));
            fsm.transform.SetParent(transform, false);
            fsm.Initialized(this, cc, anim, RPC_RunMotion, weapon);

            animEventer.Initialize(fsm.OnAnimEvent);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_RunMotion(string motionName, int startTick, float transitionTime)
        {
            fsm?.anim.CrossFadeInFixedTime(motionName, transitionTime, 0, 0);
        }

        public void SetStateEvent(PlayerFSM.StateType stateType)
        {
            if (!HasStateAuthority) return;
            fsm?.SetState(stateType);
        }

        private void HitEvent(HitInfo hitInfo)
        {
            EventDispatcher.Instance.SetStateEvent(userRef, PlayerFSM.StateType.Hit);
        }

        public override void Render()
        {
            fsm?.UpdateRender(Runner.Tick, Runner.DeltaTime);
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
