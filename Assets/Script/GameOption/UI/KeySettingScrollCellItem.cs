using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameOption
{
    public class KeySettingScrollCellItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private TMP_InputField inputField;

        private KeyBindingData keyBindingData;

        private Action onShowKeyListEvent;

        public void Initialize(KeyBindingData keyBindingData, Action showKeyListEvent)
        {
            this.keyBindingData = keyBindingData;
            onShowKeyListEvent = showKeyListEvent;

            SetListener();
        }

        private void SetListener()
        {
            //inputField.onDeselect.AddListener((str) => Debug.Log($"Test - last string: {str}"));
            //inputField.onEndEdit.AddListener((str) => Debug.Log($"Test - last string: {str}"));
            //inputField.onValueChanged.AddListener((str) => Debug.Log($"Test - revised string: {str}"));
        }

        public void OnClickKeySettingEvent()
        {

        }
    }
}
