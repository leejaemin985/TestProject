using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOption
{
    public class KeySettingScrollCellItem : MonoBehaviour
    {
        [SerializeField] private RectTransform keySettingRect;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private TMP_Text keySettingText;

        public RectTransform KeySettingRect => keySettingRect;

        public KeyBindingInfo keyBindingInfo { get; private set; }

        private Action<KeySettingScrollCellItem> keyResettingEvent;


        public void Initialize(KeyBindingInfo keyBindingInfo, Action<KeySettingScrollCellItem> keySettingEventListener)
        {
            BindingData(keyBindingInfo);
            this.keyResettingEvent = keySettingEventListener;
        }

        public void SetActive(bool set)
        {
            gameObject.SetActive(set);
        }

        private void BindingData(KeyBindingInfo info)
        {
            this.keyBindingInfo = info;

            actionText.text = info.DisplayName;
            string path = string.IsNullOrEmpty(info.CurrentKeyPath) ? null : info.CurrentKeyPath.Substring(info.CurrentKeyPath.IndexOf('/') + 1);
            keySettingText.text = path;
            keySettingText.color = Color.white;
        }

        public void OnClickKeySettingEvent()
        {
            keyResettingEvent?.Invoke(this);
        }

        public void Refresh()
        {
            BindingData(keyBindingInfo);
        }

        public void SetAssignText()
        {
            keySettingText.text = "Press a key to assign.";
            keySettingText.color = Color.white * .5f;
        }
    }
}
