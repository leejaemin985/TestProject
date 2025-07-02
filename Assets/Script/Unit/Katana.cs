using Physics;
using System;
using Unit;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private AttackBox collisionBox;

    public void Initialize(Action<CollisionInfos> hitEvent = null, PhysicsObject userPhysicsObject = null)
    {
        collisionBox.Initialize(hitEvent);
        collisionBox.AddIgnoreUid(userPhysicsObject);
    }

    public void SetCollisionActive(bool set)
    {
        collisionBox.SetActive(set);
    }
}