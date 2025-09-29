using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;

using CustomPhysics;
using Addressable;
using InGame.Weapon;
using Utility.EffectObject;
using InGame;
using System.Linq;

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

        [Header("InGame")]
        [SerializeField] private InGameCamManager camManager;

        private IWeapon weapon;
        private Animator modelAnim;
        private PlayerAnimEventer animEventer;

        private AddressableAsset_UserModelSettingInfo modelSettingInfo;
        private AddressableAsset_WeaponSettingInfo weapSettingInfo;

        public bool canControll { get; private set; }

        public HitBox playerHitBox { get; private set; }

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

            playerHitBox = InitPlayerHitBox();
            this.weapon.AddIgnorePhysics(playerHitBox);


            fsm.Initialized(this, cc, modelAnim, latencyInterpolationAnim, weapon);
            animEventer.Initialize(fsm.AnimEvent);

            if (HasStateAuthority)
            {
                camManager.Initialize(Camera.main, transform, modelSettingInfo.defaultCamPos, modelSettingInfo.actionCamPos);
                fsm.AddChangeStateListener(camManager.ChangeUserStateListener);
            }
        }

        private async Task LoadAssets()
        {
            await ModelAssetSetting();
            await WeaponAssetSetting();
        }

        private async Task ModelAssetSetting()
        {
            var modelAsset = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_SamuraiModel);
            var model = Instantiate(modelAsset);
            modelSettingInfo = model.GetComponent<AddressableAsset_UserModelSettingInfo>();

            model.transform.SetParent(transform);
            model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            modelAnim = model.GetComponent<Animator>();
            modelAnim.runtimeAnimatorController = latencyInterpolationAnim.runtimeAnimatorController;
            animEventer = model.AddComponent<PlayerAnimEventer>();
        }

        private async Task WeaponAssetSetting()
        {
            var weaponAsset = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_Katana);
            var weapon = Instantiate(weaponAsset);
            weapSettingInfo = weapon.GetComponent<AddressableAsset_WeaponSettingInfo>();


            weapon.transform.SetParent(modelSettingInfo.weaponParentTransform);
            weapon.transform.SetLocalPositionAndRotation(weapSettingInfo.weaponLocalPos, Quaternion.Euler(weapSettingInfo.weaponLocalRot));

            var slashEffectPool =
            EffectObjectPool.CreatePoolInstance(
                weapSettingInfo.slashEffect,
                new() { count = 10, effectRoot = transform });

            var parrignEffectPool =
                EffectObjectPool.CreatePoolInstance(
                    weapSettingInfo.parringEffect,
                    new() { count = 10, effectRoot = null });

            this.weapon = WeaponBase.CreateInstance<Katana>(
                weapon,
                (AttackBox)weapSettingInfo.collisionBox,
                weapSettingInfo.trailParticle,
                slashEffectPool, parrignEffectPool,
                new()
                {
                    { WeaponBase.WeapSEType.Whoosh, weapSettingInfo.whooshSoundClips.ToList() }
                });

            var finalPos = weapSettingInfo.weaponLocalPos + weapSettingInfo.collisionBox.transform.localPosition;
            var finalRot = Quaternion.Euler(weapSettingInfo.weaponLocalRot) * Quaternion.Euler(weapSettingInfo.collisionBox.transform.localEulerAngles);

            weapSettingInfo.collisionBox.transform.SetParent(latencyInterpolationWeapPos);
            weapSettingInfo.collisionBox.transform.SetLocalPositionAndRotation(finalPos, finalRot);
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

                case PlayerFSM.HitResultType.Died:
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
    }
}
