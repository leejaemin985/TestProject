using Physics;
using System;
using Unit;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private AttackBox collisionBox;

    public void Initialize(PhysicsObject userPhysicsObject, Action<HitInfos> hitEvent = null)
    {
        collisionBox.Initialize(hitEvent);
        collisionBox.SetIgnoreUid(userPhysicsObject);
    }

    public void SetCollisionActive(bool set)
    {
        collisionBox.SetActive(set);
    }
}