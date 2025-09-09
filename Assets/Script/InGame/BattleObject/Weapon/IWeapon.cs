using CustomPhysics;

public interface IWeapon
{
    public bool collisionActive { get; }

    public void AddIgnorePhysics(PhysicsObject newIgnorePhysicsObject);

    public void SetCollisionActive(bool set);

    public void SetHitInfo(HitInfo hitInfo);

    public void SetSlashEffectActive(bool set);
}