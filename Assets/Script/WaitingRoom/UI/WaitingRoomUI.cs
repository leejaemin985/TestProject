using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUI : MonoBehaviour
{
    [SerializeField] private TMP_Text GameEntryButtonText = default;
    [SerializeField] private RectTransform opponentStateRect = default;
    [SerializeField] private Image opponentReadyImage = default;

    private Color readyColor = new Color(1, 1, 1, 1);
    private Color unreadyColor = new Color(.1f, .1f, .1f, 1);


    public Action onClickedGameEntryButtonListener { get; set; }
    public Action onClickedExitButtonListener { get; set; }


    public void Initialize()
    {
        SetText();
        opponentStateRect.gameObject.SetActive(GameNetworkManager.Instance.runner.IsSharedModeMasterClient);
    }

    private void SetText()
    {
        GameEntryButtonText.text = GameNetworkManager.Instance.runner.IsSharedModeMasterClient ? "Start" : "Ready";
    }

    public void OnClickedGameEntryButtonEvent() => onClickedGameEntryButtonListener?.Invoke();

    public void OnClickedExitButtonEvent() => onClickedExitButtonListener?.Invoke();

    public void SetOpponentReadyCheck(bool isReady)
    {
        opponentReadyImage.color = isReady ? readyColor : unreadyColor;
    }
}
