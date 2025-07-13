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

        private Action<string> onAnimEvent;

        public void Initialize(Action<string> onAnimEvent)
        {
            this.onAnimEvent = onAnimEvent;
        }

        public void OnEvent(string param) => onAnimEvent(param);
    }
}