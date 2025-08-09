using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject makeRoomPopup = default;
    [SerializeField] private TMP_InputField sessionNameInputField = default;

    public Action onClickQuickStartListener { get; set; }
    public Action onClickMakeRoomPopupListener { get; set; }

    public Action onClickMakeRoomPopupConfirmListener { get; set; }
    public Action onClickMakeRoomPopupCancelListener { get; set; }

    public void OnClickQuickStartEvent() => onClickQuickStartListener?.Invoke();
    public void OnClickMakeRoomPopupEvent() => onClickMakeRoomPopupListener?.Invoke();

    public void OnClickMakeRoomPopupConfirmEvent() => onClickMakeRoomPopupConfirmListener?.Invoke();
    public void OnClickMakeRoomPopupCancelEvent() => onClickMakeRoomPopupCancelListener?.Invoke();

    public string GetSessionNameInputFieldText() => sessionNameInputField.text;

    public void SetSessionNameInputFieldText(string text) => sessionNameInputField.text = text;

    public void SetMakeRoomPopup(bool set) => makeRoomPopup.SetActive(set);


}
