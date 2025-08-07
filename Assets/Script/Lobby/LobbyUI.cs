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
    public Action onClickMakeRoomListener { get; set; }

    public Action onClickMakeRoomConfirmListener { get; set; }
    public Action onClickMakeRoomCancelListener { get; set; }

    public void OnClickQuickStartEvent() => onClickQuickStartListener?.Invoke();
    public void OnClickMakeRoomEvent() => onClickMakeRoomListener?.Invoke();

    public void OnClickMakeRoomConfirmEvent() => onClickMakeRoomConfirmListener?.Invoke();
    public void OnClickMakeRoomCancelEvent() => onClickMakeRoomCancelListener?.Invoke();

    public string GetSessionNameInputFieldText() => sessionNameInputField.text;

    public void SetMakeRoomPopup(bool set) => makeRoomPopup.SetActive(set);


}
