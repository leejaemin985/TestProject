using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Fusion;

namespace WaitingRoom.UI
{
    public class WaitingRoomUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text sessionName;
        [SerializeField] private Image gameEntryButton;
        [SerializeField] private TMP_Text gameEntryButtonText;

        [SerializeField] private RectTransform opponentStateRect;
        [SerializeField] private TMP_Text opponentStateText;
        [SerializeField] private Image opponentReadyImage;

        [SerializeField] private Color[] readyButtonColors;
        [SerializeField] private Color[] userReadyCheckColors;

        public Action onClickedGameEntryButtonListener { get; set; }
        public Action onClickedExitButtonListener { get; set; }


        public void SetSessionInfo(SessionInfo info)
        {
            sessionName.text = SessionMetaReader.GetWithoutUidSessionName(info.Name);
        }

        public void OnClickedGameEntryButtonEvent() => onClickedGameEntryButtonListener?.Invoke();

        public void OnClickedExitButtonEvent() => onClickedExitButtonListener?.Invoke();


        public void SetUserSlotActive(bool set) => gameEntryButton.gameObject.SetActive(set);

        public void SetOpponentSlotActive(bool set) => opponentStateRect.gameObject.SetActive(set);


        public void SetGameEntryButton(bool isReady)
        {
            // 현재상태가 Ready -> Unready 색상, Text
            gameEntryButton.color = readyButtonColors[isReady ? 0 : 1];
            gameEntryButtonText.text = isReady ? "UnReady" : "Ready";
        }

        public void SetOpponentReadyState(bool isReady)
        {
            opponentStateText.color = userReadyCheckColors[isReady ? 1 : 0];
            opponentReadyImage.color = userReadyCheckColors[isReady ? 1 : 0];
        }
    }
}
