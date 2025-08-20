using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUI : MonoBehaviour
{
    [SerializeField] private Image gameEntryButton = default;
    [SerializeField] private TMP_Text gameEntryButtonText = default;

    [SerializeField] private RectTransform opponentStateRect = default;
    [SerializeField] private TMP_Text opponentStateText = default;
    [SerializeField] private Image opponentReadyImage = default;

    [SerializeField] private Color[] readyButtonColors = default;
    [SerializeField] private Color[] userReadyCheckColors = default;


    public Action onClickedGameEntryButtonListener { get; set; }
    public Action onClickedExitButtonListener { get; set; }


    public void Initialize()
    {
        // Default Settings
        SetGameEntryButton(false);
        SetOpponentSlotActive(false);
        SetOpponentReadyState(false);
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
