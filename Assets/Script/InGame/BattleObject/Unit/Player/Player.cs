using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;

using CustomPhysics;
using Addressable;

namespace Unit
{
    public class Player : Unit
    {
        #region Spawned Callback
        private static Dictionary<PlayerRef, Player> registedUsers = new();
        public static IReadOnlyDictionary<PlayerRef, Player> RegistedUsers => registedUsers;

        private static void RegisterUser(PlayerRef userRef, Player player) => registedUsers[userRef] = player;

        private static void UnregisterUser(PlayerRef userRef) => registedUsers.Remove(userRef);
        #endregion

        [Header("Player")]
        [SerializeField] private SimpleKCC cc;

        [SerializeField] private PlayerFSM fsm;
        [SerializeField] private PlayerInteractionEventHandler interactionEventHandler;

        [SerializeField] private Animator latencyInterpolationAnim;
        [SerializeField] private Transform latencyInterpolationWeapPos;

        private IWeapon weapon;
        private Animator modelAnim;
        private PlayerAnimEventer animEventer;

        [SerializeField] private PlayerCam playerCam;


        public bool canControll { get; private set; }

        private HitBox playerHitBox;

        private IEnumerator CamSettingHandle = default;
        
        public override void Spawned()
        {
            RegisterUser(Object.StateAuthority, this);
            Initialize();

            UnitStat.AddSpawnedCallback(Object.StateAuthority, BindUnitStat);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            UnregisterUser(Object.StateAuthority);
        }

        protected async override void Initialize()
        {
            base.Initialize();

            await LoadAssets();
            
            StartCamSet();
            
            playerHitBox = InitPlayerHitBox();

            //weapon.Initialize(playerHitBox);
            //weapon.SetCollisionPos(latencyInterpolationWeapPos);
            
            fsm.Initialized(this, cc, modelAnim, latencyInterpolationAnim, weapon);
            animEventer.Initialize(fsm.AnimEvent);
        }

        private async Task LoadAssets()
        {
            var modelAsset = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_SamuraiModel);
            var weaponAsset = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_Katana);
            
            var model = Instantiate(modelAsset);
            model.transform.SetParent(transform);
            model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            modelAnim = model.GetComponent<Animator>();
            modelAnim.runtimeAnimatorController = latencyInterpolationAnim.runtimeAnimatorController;
            animEventer = model.AddComponent<PlayerAnimEventer>();

            //var settingInfo = model.GetComponent<AddressableAsset_UserModelSettingInfo>();
            //var weapon = Instantiate(weaponAsset);
            //weapon.transform.SetParent(settingInfo.weaponParentTransform);
            //weapon.transform.SetLocalPositionAndRotation(settingInfo.weaponLocalPos, Quaternion.Euler(settingInfo.weaponLocalRot));
        }



        public void SetCanController(bool set) => canControll = set;

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

        #region Event
        private void HitEvent(CollisionInfoData collisionInfoData)
        {
            HitInfo hitInfo = ((PlayerCollisionInfo)collisionInfoData).hitInfo;

            PlayerFSM.HitResultType result = fsm.CheckHittable(hitInfo);
            switch (result)
            {
                case PlayerFSM.HitResultType.Hit:
                    interactionEventHandler.RequestOnHitUser(Object.StateAuthority, hitInfo);
                    break;

                case PlayerFSM.HitResultType.Parry:
                    interactionEventHandler.RequestOnParringUser(Object.StateAuthority, hitInfo);
                    break;

                default: // died
                    interactionEventHandler.RequestOnDiedUser(Object.StateAuthority, hitInfo);
                    break;
            }
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

            playerHitBox?.SetActive(false);
        }

        #endregion

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
    }
}
