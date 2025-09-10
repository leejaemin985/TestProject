using CustomPhysics;
using UnityEngine;

public interface IWeapon
{
    public bool collisionActive { get; }

    public void AddIgnorePhysics(PhysicsObject newIgnorePhysicsObject);

    public void SetCollisionActive(bool set);

    public void SetHitInfo(HitInfo hitInfo);

    public void SetTrailEffectActive(bool set);

    public void SetSlashEffectActive(Vector3 localPos, Quaternion localRot);
}