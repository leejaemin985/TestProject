using System;
using TMPro;
using UnityEngine;

namespace GameOption
{
    public class KeySettingGameOptionUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField keyListInputField;

        public Action<string> inputFieldDeselectListener { get; set; }

        public void Initialize()
        {
            SetListener();
        }

        private void SetListener()
        {
            keyListInputField.onDeselect.AddListener(InputFieldDeselectEvent);
        }

        private void InputFieldDeselectEvent(string text) => inputFieldDeselectListener?.Invoke(text);

        public void SetKeyListActive(bool set)
        {
            keyListInputField.gameObject.SetActive(set);
            keyListInputField.Select();
        }

        public void SetPosition(KeySettingScrollCellItem item)
        {
            keyListInputField.GetComponent<RectTransform>().anchoredPosition = item.KeySettingRect.anchoredPosition;
        }
    }
}