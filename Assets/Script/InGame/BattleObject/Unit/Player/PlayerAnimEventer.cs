using System;
using UnityEngine;
using InGame.Event;

namespace Unit
{
    public class PlayerAnimEventer : MonoBehaviour
    {
        private Action<AnimationEventData> onAnimEvent;


        public void Initialize(Action<AnimationEventData> test)
        {
            this.onAnimEvent = test;
        }

        public void OnAnimEvent(AnimationEventData param) => onAnimEvent?.Invoke(param);
    }
}