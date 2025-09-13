using UnityEngine;
using CustomPhysics;
using Unit;
using System;
using Utility.EffectObject;

namespace InGame.Weapon
{
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        private AttackBox collisionBox;

        private const float SLASH_PARTICLE_ACTIVE_VALUE = 200;

        private ParticleSystem trailParticle;
        private EffectObjectPool slashEffectPool;
        private EffectObjectPool parringEffectPool;
        private HitInfo hitInfo;

        public static T CreateInstance<T>(
            GameObject modelPrefab,
            AttackBox collisionBox,
            ParticleSystem slashParticleObject,
            EffectObjectPool slashEffectPool,
            EffectObjectPool parringEffectPool) where T : WeaponBase
        {
            var ret = modelPrefab.AddComponent<T>();
            ret.Initialize(collisionBox, slashParticleObject, slashEffectPool, parringEffectPool);

            return ret;
        }

        protected virtual void Initialize(AttackBox attackBox, ParticleSystem slashParticle, EffectObjectPool slashEffectPool, EffectObjectPool parringEffectPool)
        {
            this.collisionBox = attackBox;
            this.trailParticle = slashParticle;
            this.slashEffectPool = slashEffectPool;
            this.slashEffectPool.transform.SetParent(transform);
            this.parringEffectPool = parringEffectPool;
            this.parringEffectPool.transform.SetParent(transform);

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
            trailParticle.Play();
            SetTrailEffectActive(false);
        }

        protected virtual void SetCollisionActive(bool set) => collisionBox.SetActive(set);

        protected virtual void SetHitInfo(HitInfo hitInfo) => this.hitInfo = hitInfo;

        protected virtual void SetTrailEffectActive(bool set)
        {
            if (trailParticle == null) return;
            var emission = trailParticle.emission;
            emission.rateOverTime = set ? SLASH_PARTICLE_ACTIVE_VALUE : 0;
        }

        protected virtual void SetSlashEffectActive(Vector3 localPos, Quaternion localRot)
        {
            slashEffectPool?.OnPlayEffect(localPos, localRot);
        }

        protected virtual void SetParringEffectActive(Vector3 localPos, Quaternion localRot)
        {
            parringEffectPool?.OnPlayEffect(localPos, localRot);
        }

        #region IWeapon
        bool IWeapon.collisionActive => collisionBox.Active;

        void IWeapon.AddIgnorePhysics(PhysicsObject newIgnorePhysics) => AddIgnorePhsics(newIgnorePhysics);

        void IWeapon.SetCollisionActive(bool set) => SetCollisionActive(set);

        void IWeapon.SetHitInfo(HitInfo hitInfo) => SetHitInfo(hitInfo);

        void IWeapon.SetTrailEffectActive(bool set) => SetTrailEffectActive(set);

        void IWeapon.SetSlashEffectActive(Vector3 localPos, Quaternion localRot) => SetSlashEffectActive(localPos, localRot);

        void IWeapon.SetParringEffectActive(Vector3 localPos, Quaternion localRot) => SetParringEffectActive(localPos, localRot);
        #endregion
    }
}