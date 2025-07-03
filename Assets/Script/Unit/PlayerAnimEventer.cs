using System;
using System.Linq;
using UnityEngine;

namespace Unit
{
    public class PlayerAnimEventer : MonoBehaviour
    {
        private Action<bool> weapCollisionController;
        private Action<Vector2, bool> onMoveAction;

        public void Initialize(Action<bool> weapCollisionController, Action<Vector2, bool> onMoveAction)
        {
            this.weapCollisionController = weapCollisionController;
            this.onMoveAction = onMoveAction;
        }

        public void OnWeapCollider()
        {
            weapCollisionController?.Invoke(true);
        }

        public void OffWeapCollider()
        {
            weapCollisionController?.Invoke(false);
        }

        public void SetPlayerMove(string direction)
        {
            string[] moveVector = direction
                .Split(',')
                .Select(p=>p.Trim())
                .ToArray();

            if (moveVector.Length == 2 &&
                float.TryParse(moveVector[0], out float x) &&
                float.TryParse(moveVector[1], out float z))
            {
                Vector2 moveDir = new Vector2(x, z);
                onMoveAction?.Invoke(moveDir, true);
            }

        }
    }
}