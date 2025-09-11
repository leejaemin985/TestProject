using System;
using System.Collections;
using UnityEngine;
using Utility.EffectObject;

namespace InGame.Weapon
{
    public class WeaponSlashEffect : EffectObject
    {
        [SerializeField] private ParticleSystem[] slashParticleRing;

        private IEnumerator effectLifeTimeHandle;
        private readonly WaitForSeconds LIFE_TIME = new WaitForSeconds(1);


        protected override void OnEffect(Action completeListener)
        {
            foreach (var particle in slashParticleRing)
                particle.Play();

            if (effectLifeTimeHandle != null) StopCoroutine(effectLifeTimeHandle);
            StartCoroutine(effectLifeTimeHandle = EffectLifeTime(completeListener));
        }

        protected override void OffEffect()
        {
            foreach (var particle in slashParticleRing)
                particle.Stop();
        }

        private IEnumerator EffectLifeTime(Action completeListener)
        {
            yield return LIFE_TIME;

            completeListener?.Invoke();
        }
    }
}