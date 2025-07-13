using UnityEngine;

namespace InputSystem
{
    public interface ITouchableUI
    {
        bool HitTest(Vector2 screenPosition);

        void OnTouchEvent(int fingerId, TouchPhase phase, Vector2 position);
    }
}