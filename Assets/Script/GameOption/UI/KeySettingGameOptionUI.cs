using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace GameOption
{
    public class KeySettingGameOptionUI : MonoBehaviour
    {
        [SerializeField] private GameObject uiObject;
        [SerializeField] private Slider mouseSensitiveSlider;
        [SerializeField] private TMP_Text mouseSensitiveText;

        private Func<float> getMouseSensituveEvent;
        private Action<float> onChangedMouseSensitiveListener;

        public void Initialize(Func<float> getMouseSensitive, Action<float> onChangedMouseSensitiveListener)
        {
            this.getMouseSensituveEvent = getMouseSensitive;
            this.onChangedMouseSensitiveListener = onChangedMouseSensitiveListener;

            float initValue = getMouseSensitive == null ? 0 : getMouseSensitive.Invoke();
            mouseSensitiveSlider.value = initValue;
            mouseSensitiveText.text = initValue.ToString();

            mouseSensitiveSlider.onValueChanged.AddListener(SetMouseSensitive);
        }

        public void PanelSetActive(bool set) => uiObject.SetActive(set);

        private void SetMouseSensitive(float value)
        {
            onChangedMouseSensitiveListener?.Invoke(value);

            mouseSensitiveText.text = Mathf.RoundToInt(value * 100).ToString();
        }
    }
}