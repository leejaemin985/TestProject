using System;
using TMPro;
using UnityEngine;

namespace GameOption
{
    public class KeySettingScrollCellItem : MonoBehaviour
    {
        [SerializeField] private RectTransform keySettingRect;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private TMP_Text keySettingText;

        public RectTransform KeySettingRect => keySettingRect;

        private KeyBindingInfo keyBindingInfo;

        private Action<KeyBindingInfo> keyResettingEvent;


        public void Initialize(KeyBindingInfo keyBindingInfo, Action<KeyBindingInfo> keySettingEventListener)
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
        }

        public void OnClickKeySettingEvent()
        {
            keyResettingEvent?.Invoke(keyBindingInfo);
        }
    }
}
