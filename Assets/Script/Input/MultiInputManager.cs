using System.Collections.Generic;
using UnityEngine;

namespace InputSystem
{
    public class MultiInputManager : MonoBehaviour
    {
        public class TouchInfo
        {
            public int FingerId;
            public Vector2 StartPosition;
            public Vector2 CurrentPosition;
            public TouchPhase Phase;
        }

        private List<ITouchableUI> registeredUIs = new();
        private Dictionary<int, ITouchableUI> uiTouchBinding = new(); // fingerId ¡æ UI


        public void RegisterTouchableUI(ITouchableUI ui)
        {
            if (!registeredUIs.Contains(ui))
                registeredUIs.Add(ui);
        }

        public void UnregisterTouchableUI(ITouchableUI ui)
        {
            registeredUIs.Remove(ui);
        }

        void Update()
        {
            foreach (Touch touch in Input.touches)
            {
                int id = touch.fingerId;
                Vector2 pos = touch.position;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        foreach (var ui in registeredUIs)
                        {
                            if (ui.HitTest(pos))
                            {
                                uiTouchBinding[id] = ui;
                                ui.OnTouchEvent(id, touch.phase, pos);
                                break;
                            }
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (uiTouchBinding.TryGetValue(id, out var boundUI))
                        {
                            boundUI.OnTouchEvent(id, touch.phase, pos);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (uiTouchBinding.TryGetValue(id, out var endUI))
                        {
                            endUI.OnTouchEvent(id, touch.phase, pos);
                            uiTouchBinding.Remove(id);
                        }
                        break;
                }
            }
        }
    }
}