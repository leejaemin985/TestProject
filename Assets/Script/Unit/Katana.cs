using Physics;
using System;
using Unit;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private AttackBox collisionBox;
    private HitInfo hitInfo;


    public void Initialize(bool isMasterClient, PhysicsObject userPhysicsObject, Action<CollisionInfos> hitEvent = null)
    {
        if (isMasterClient)
        {
            collisionBox.gameObject.SetActive(true);
            collisionBox.Initialize(OnHit);
            collisionBox.SetActive(true);
            collisionBox.AddIgnoreUid(userPhysicsObject);
        }
        else
        {
            collisionBox.gameObject.SetActive(false);
        }
    }

    public void SetCollisionActive(bool set) => collisionBox.SetActive(set);

    public void SetHitInfo(HitInfo hitInfo) => this.hitInfo = hitInfo;


    private void OnHit(CollisionInfos collisionInfos)
    {
        foreach (var info in collisionInfos.collisionInfos)
        {
            info.hitObject.OnHitEvent(this.hitInfo);
        }
    }
}