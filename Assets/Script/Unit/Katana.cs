using Physics;
using System;
using Unit;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private AttackBox collisionBox;
    [SerializeField] private GameObject slashParticleObject;
    
    private HitInfo hitInfo;
    public bool collisionActive => collisionBox.Active;

    private const float SLASH_PARTICLE_ACTIVE_VALUE = 200;
    private ParticleSystem slashParticle;

    public void Initialize(bool isMasterClient, PhysicsObject userPhysicsObject)
    {
        if (isMasterClient)
        {
            collisionBox.gameObject.SetActive(true);
            collisionBox.Initialize(OnHit);
            collisionBox.AddIgnoreUid(userPhysicsObject);
        }
        else
        {
            collisionBox.gameObject.SetActive(false);
        }

        InitEffect();
    }

    public void SetCollisionActive(bool set)
    {
        collisionBox.SetActive(set);
    }

    public void SetHitInfo(HitInfo hitInfo) => this.hitInfo = hitInfo;

    private void OnHit(CollisionInfos collisionInfos)
    {
        foreach (var info in collisionInfos.collisionInfos)
        {
            info.hitObject.OnHitEvent(this.hitInfo);
        }
    }

    private void InitEffect()
    {
        slashParticle = slashParticleObject.GetComponent<ParticleSystem>();
        slashParticle.Play();
        SetSlashParticleActive(false);
    }

    public void SetSlashParticleActive(bool set)
    {
        if (slashParticle == null) return;
        var emission = slashParticle.emission;
        emission.rateOverTime = set ? SLASH_PARTICLE_ACTIVE_VALUE : 0;
    }
}