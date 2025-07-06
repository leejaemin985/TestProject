using System;
using System.Linq;
using UnityEngine;
using WebSocketSharp;

namespace Unit
{
    public class PlayerAnimEventer : MonoBehaviour
    {
        private Action<bool> weapCollisionController;
        private Action<Vector3> onAttackMoveAction;

        public void Initialize(Action<bool> weapCollisionController, Action<Vector3> onAttackMoveAction)
        {
            this.weapCollisionController = weapCollisionController;
            this.onAttackMoveAction = onAttackMoveAction;
        }

        public void OnWeapCollider()
        {
            weapCollisionController?.Invoke(true);
        }

        public void OffWeapCollider()
        {
            weapCollisionController?.Invoke(false);
        }

        public void SetPlayerAttackMove(string direction)
        {
            if (direction.IsNullOrEmpty() || direction.Equals("0")) onAttackMoveAction(Vector3.zero);

            string[] moveVector = direction
                .Split(',')
                .Select(p => p.Trim())
                .ToArray();

            if (moveVector.Length == 3 &&
                float.TryParse(moveVector[0], out float x) &&
                float.TryParse(moveVector[1], out float y) &&
                float.TryParse(moveVector[2], out float z))
            {
                Vector3 moveDir = new Vector3(x, y, z);
                onAttackMoveAction?.Invoke(moveDir);
            }
        }
    }
}