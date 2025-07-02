using Physics;
using System;
using UnityEngine;

namespace Unit
{
    public class PlayerHit : MonoBehaviour
    {
        [SerializeField] private HitBox hitBox;

        public void Initialize(Action<HitInfo> hitEvent)
        {
            hitBox.Initialize(hitEvent);
            hitBox.SetActive(true);
        }

        public PhysicsObject GetPhysicsBox() => hitBox;
    }
}