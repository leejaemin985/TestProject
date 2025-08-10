using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaitingRoomUI : MonoBehaviour
{
    [SerializeField] private TMP_Text GameEntryButtonText = default;

    public Action onClickedGameEntryButtonListener { get; set; }
    public Action onClickedExitButtonListener { get; set; }


    public void Initialize()
    {
        SetText();

    }

    private void SetText()
    {
        GameEntryButtonText.text = GameNetworkManager.Instance.runner.IsSharedModeMasterClient ? "Start" : "Ready";
    }

    public void OnClickedGameEntryButtonEvent() => onClickedGameEntryButtonListener?.Invoke();

    public void OnClickedExitButtonEvent() => onClickedExitButtonListener?.Invoke();

}
