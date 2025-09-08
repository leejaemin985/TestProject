using UnityEngine;
using CustomPhysics;
using Unit;

namespace InGame.Weapon
{
    public class WeaponBase : MonoBehaviour, IWeapon
    {
        [SerializeField] private AttackBox collisionBox;
        [SerializeField] private GameObject slashParticleObject;

        private const float SLASH_PARTICLE_ACTIVE_VALUE = 200;

        private ParticleSystem slashParticle;
        private HitInfo hitInfo;

        public virtual void Initialize(PhysicsObject userPhysicsObject)
        {
            collisionBox.gameObject.SetActive(true);
            collisionBox.Initialize(OnHit);
            collisionBox.AddIgnoreUid(userPhysicsObject);

            InitEffect();
        }

        private void OnHit(CollisionInfos collisionInfos)
        {
            foreach (var info in collisionInfos.collisionInfos)
            {
                info.hitObject.OnHitEvent(new PlayerCollisionInfo(info, this.hitInfo));
            }
        }

        private void InitEffect()
        {
            slashParticle = slashParticleObject.GetComponent<ParticleSystem>();
            slashParticle.Play();
            SetSlashEffectActive(false);
        }

        public virtual void SetCollisionActive(bool set) => collisionBox.SetActive(set);

        public virtual void SetHitInfo(HitInfo hitInfo) => this.hitInfo = hitInfo;

        public virtual void SetSlashEffectActive(bool set)
        {
            if (slashParticle == null) return;
            var emission = slashParticle.emission;
            emission.rateOverTime = set ? SLASH_PARTICLE_ACTIVE_VALUE : 0;

        }

        #region IWeapon
        bool IWeapon.collisionActive => collisionBox.Active;

        void IWeapon.Initialize(PhysicsObject userPhysicsObject) => Initialize(userPhysicsObject);

        void IWeapon.SetCollisionActive(bool set) => SetCollisionActive(set);

        void IWeapon.SetHitInfo(HitInfo hitInfo) => SetHitInfo(hitInfo);

        void IWeapon.SetSlashEffectActive(bool set) => SetSlashEffectActive(set);
        #endregion
    }
}