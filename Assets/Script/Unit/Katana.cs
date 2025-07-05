using Physics;
using System;
using Unit;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private AttackBox collisionBox;
    private Func<bool> isHitValid;
    private HitInfo hitInfo;

    public void Initialize(PhysicsObject userPhysicsObject, Func<bool> isHitValid, Action<CollisionInfos> hitEvent = null)
    {
        collisionBox.Initialize(OnHit);

        this.isHitValid = isHitValid;
        collisionBox.AddIgnoreUid(userPhysicsObject);
    }

    public void SetCollisionActive(bool set) => collisionBox.SetActive(set);

    public void SetHitInfo(HitInfo hitInfo) => this.hitInfo = hitInfo;


    private void OnHit(CollisionInfos collisionInfos)
    {
        if (isHitValid != null && isHitValid.Invoke() == false) return;

        foreach (var info in collisionInfos.collisionInfos)
        {
            info.hitObject.OnHitEvent(this.hitInfo);
        }
    }
}