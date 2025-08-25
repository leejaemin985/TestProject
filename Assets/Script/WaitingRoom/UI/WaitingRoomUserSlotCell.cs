using UnityEngine;
using UnityEngine.UI;

namespace WaitingRoom.UI
{
    public class WaitingRoomUserSlotCell : MonoBehaviour
    {
        [SerializeField] private Image readyStateImage = default;

        private Color readyColor = new Color(1, 1, 1, 1);
        private Color unreadyColor = new Color(.1f, .1f, .1f, 1);

        private void Start() => SetReadyState(false);

        public void SetReadyState(bool isReady)
        {
            readyStateImage.color = isReady ? readyColor : unreadyColor;
        }
    }
}
