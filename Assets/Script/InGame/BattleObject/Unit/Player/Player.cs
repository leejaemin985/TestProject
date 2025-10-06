using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;

using CustomPhysics;
using Utility.EffectObject;

using InGame;
using InGame.Weapon;
using Addressable;
using Utility.Sound;

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

        private ISoundObject playerSoundObejct;

        private AddressableObject_UserModelSettingInfo userModelSettingInfo;
        private AddressableObject_WeaponSettingInfo weapSettingInfo;

        private IWeapon weapon;
        private Animator modelAnim;
        private PlayerAnimEventer animEventer;

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

            if (RegistedUsers == null || RegistedUsers.Count == 0) InGamePlayerResourcesLoader.Dispose();
        }

        protected async override void Initialize()
        {
            base.Initialize();

            await LoadAssets();

            playerHitBox = InitPlayerHitBox();
            this.weapon.AddIgnorePhysics(playerHitBox);

            var effectSoundObject = new GameObject("PlayerSoundObject").AddComponent<GameEffectSoundObject>();
            GameAudioMixerController.Instance.InitSoundObject(effectSoundObject);
            effectSoundObject.transform.SetParent(transform);
            effectSoundObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            playerSoundObejct = effectSoundObject;

            fsm.Initialized(this, cc, modelAnim, latencyInterpolationAnim, playerSoundObejct, weapon);
            animEventer.Initialize(fsm.AnimEvent);

            if (HasStateAuthority)
            {
                camManager.Initialize(
                    Camera.main,
                    transform,
                    userModelSettingInfo.defaultCamPos,
                    userModelSettingInfo.actionCamPos);
                fsm.AddChangeStateListener(camManager.ChangeUserStateListener);
            }
        }

        private async Task LoadAssets()
        {
            await InGamePlayerResourcesLoader.LoadAssets();
            await ModelAssetSetting();
            await WeaponAssetSetting();
        }

        private async Task ModelAssetSetting()
        {
            var modelObject = Instantiate(InGamePlayerResourcesLoader.modelObjectAsset);
            userModelSettingInfo = modelObject.GetComponent<AddressableObject_UserModelSettingInfo>();

            modelObject.transform.SetParent(transform);
            modelObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            modelAnim = modelObject.GetComponent<Animator>();
            modelAnim.runtimeAnimatorController = latencyInterpolationAnim.runtimeAnimatorController;
            animEventer = modelObject.AddComponent<PlayerAnimEventer>();
        }

        private async Task WeaponAssetSetting()
        {
            var weapon = Instantiate(InGamePlayerResourcesLoader.weaponAsset);
            weapSettingInfo = weapon.GetComponent<AddressableObject_WeaponSettingInfo>();

            weapon.transform.SetParent(userModelSettingInfo.weaponParentTransform);
            weapon.transform.SetLocalPositionAndRotation(
                weapSettingInfo.weaponLocalPos,
                Quaternion.Euler(weapSettingInfo.weaponLocalRot));

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
                    { WeapSoundObject.WeapSoundType.Whoosh, weapSettingInfo.whooshSoundClips.ToList() },
                    { WeapSoundObject.WeapSoundType.Collision, weapSettingInfo.collisionSoundClips.ToList() }
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

            //var hitSEArr = InGamePlayerResourcesLoader.soundPack.userHitSE;
            //playerSoundObejct.PlayOneShot(hitSEArr[Random.Range(0, hitSEArr.Length)]);
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
