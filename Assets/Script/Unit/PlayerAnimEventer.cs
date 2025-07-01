using System;
using UnityEngine;

namespace Unit
{
    public class PlayerAnimEventer : MonoBehaviour
    {
        private Action<bool> weapCollisionController;

        public void Initialize(Action<bool> weapCollisionController)
        {
            this.weapCollisionController = weapCollisionController;
        }

        public void OnWeapCollider()
        {
            weapCollisionController?.Invoke(true);
        }

        public void OffWeapCollider()
        {
            weapCollisionController?.Invoke(false);
        }
    }
}