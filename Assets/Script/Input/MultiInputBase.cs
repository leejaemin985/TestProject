using UnityEngine;
using UnityEngine.EventSystems;

namespace InputSystem
{
    public abstract class MultiInputBase : MonoBehaviour
#if UNITY_EDITOR
    , IPointerDownHandler, IDragHandler, IPointerUpHandler
#else
    ,ITouchableUI
#endif
    {
        [SerializeField] private MultiInputManager touchManager;
        [SerializeField] private RectTransform baseRect;

        private void Start()
        {
#if !UNITY_EDITOR
			touchManager.RegisterTouchableUI(this);
#endif
            Initialize();
        }

        protected virtual void Initialize() { }

        protected virtual void OnPointerUpEvent(Vector2 screenPosition) { }
        protected virtual void OnDragEvent(Vector2 screenPosition) { }
        protected virtual void OnPointerDownEvent(Vector2 screenPosition) { }

#if UNITY_EDITOR
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => OnPointerDownEvent(eventData.position);

        void IDragHandler.OnDrag(PointerEventData eventData) => OnDragEvent(eventData.position);

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => OnPointerUpEvent(eventData.position);
#else

		public virtual bool HitTest(Vector2 screenPosition)
		{
			return RectTransformUtility.RectangleContainsScreenPoint(baseRect, screenPosition, null);
		}

		public virtual void OnTouchEvent(int fingerId, TouchPhase phase, Vector2 position)
		{
			switch (phase)
			{
				case TouchPhase.Began:
					OnPointerDownEvent(position);
					break;
				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					OnDragEvent(position);
					break;
				case TouchPhase.Ended:
				case TouchPhase.Canceled:
					OnPointerUpEvent(position);
					break;
			}
		}
#endif
    }

}