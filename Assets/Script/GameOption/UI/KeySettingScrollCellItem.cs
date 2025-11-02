using System;
using System.Collections;
using System.Collections.Generic;
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

        private KeyBindingData keyBindingData;

        private Action<KeySettingScrollCellItem> onShowKeyListEvent;

        public void Initialize(KeyBindingData keyBindingData, Action<KeySettingScrollCellItem> showKeyListEvent)
        {
            BindingData(keyBindingData);
            onShowKeyListEvent = showKeyListEvent;

            SetListener();
        }

        public void SetActive(bool set)
        {
            gameObject.SetActive(set);
        }

        private void BindingData(KeyBindingData data)
        {
            this.keyBindingData = data;

            actionText.text = data.DisPlayName;
            string path = string.IsNullOrEmpty(data.CurrentKeyPath) ? null : data.CurrentKeyPath.Substring(data.CurrentKeyPath.IndexOf('/') + 1);
            keySettingText.text = path;
        }


        private void SetListener()
        {
            //inputField.onDeselect.AddListener((str) => Debug.Log($"Test - last string: {str}"));
            //inputField.onEndEdit.AddListener((str) => Debug.Log($"Test - last string: {str}"));
            //inputField.onValueChanged.AddListener((str) => Debug.Log($"Test - revised string: {str}"));
        }

        public void OnClickKeySettingEvent()
        {
            onShowKeyListEvent?.Invoke(this);
        }
    }
}
