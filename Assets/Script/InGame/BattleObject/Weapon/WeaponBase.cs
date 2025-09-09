using UnityEngine;
using CustomPhysics;
using Unit;
using System;

namespace InGame.Weapon
{
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        private AttackBox collisionBox;
        private GameObject slashParticleObject;

        private const float SLASH_PARTICLE_ACTIVE_VALUE = 200;

        private ParticleSystem slashParticle;
        private HitInfo hitInfo;

        public static T CreateInstance<T>(GameObject modelPrefab, AttackBox collisionBox, ParticleSystem slashParticleObject) where T : WeaponBase
        {
            var ret = modelPrefab.AddComponent<T>();
            ret.Initialize(collisionBox, slashParticleObject);

            return ret;
        }

        protected virtual void Initialize(AttackBox attackBox, ParticleSystem slashParticle)
        {
            this.collisionBox = attackBox;
            this.slashParticle = slashParticle;

            collisionBox.gameObject.SetActive(true);
            collisionBox.Initialize(OnHit);
            
            InitEffect();
        }

        private void AddIgnorePhsics(PhysicsObject userPhysicsObject)
        {
            collisionBox.AddIgnoreUid(userPhysicsObject);
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
            //slashParticle = slashParticleObject.GetComponent<ParticleSystem>();
            slashParticle.Play();
            SetSlashEffectActive(false);
        }

        protected virtual void SetCollisionActive(bool set) => collisionBox.SetActive(set);

        protected virtual void SetHitInfo(HitInfo hitInfo) => this.hitInfo = hitInfo;

        protected virtual void SetSlashEffectActive(bool set)
        {
            if (slashParticle == null) return;
            var emission = slashParticle.emission;
            emission.rateOverTime = set ? SLASH_PARTICLE_ACTIVE_VALUE : 0;

        }

        #region IWeapon
        bool IWeapon.collisionActive => collisionBox.Active;

        void IWeapon.AddIgnorePhysics(PhysicsObject newIgnorePhysics) => AddIgnorePhsics(newIgnorePhysics);

        void IWeapon.SetCollisionActive(bool set) => SetCollisionActive(set);

        void IWeapon.SetHitInfo(HitInfo hitInfo) => SetHitInfo(hitInfo);

        void IWeapon.SetSlashEffectActive(bool set) => SetSlashEffectActive(set);
        #endregion
    }
}