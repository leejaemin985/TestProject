using System;
using UnityEngine;
using InGame.Event;

namespace Unit
{
    public class PlayerAnimEventer : MonoBehaviour
    {
        private Action<bool> weapCollisionController;
        private Action<Vector3> onAttackMoveAction;

        private Action<string> onAnimEvent;
        private Action<AnimationEventData> onAnimEvent2;


        public void Initialize(Action<string> onAnimEvent, Action<AnimationEventData> test)
        {
            this.onAnimEvent = onAnimEvent;
            this.onAnimEvent2 = test;
        }

        public void OnEvent(string param) => onAnimEvent?.Invoke(param);

        public void OnAnimEvent(AnimationEventData param) => onAnimEvent2?.Invoke(param);
    }
}