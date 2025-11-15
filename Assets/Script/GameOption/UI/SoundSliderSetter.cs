using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOption
{
    public class SoundSliderSetter : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text volumeText;

        private Func<float> getVolume;
        private Action<float> setVolumeListener;

        public void Initialize(Func<float> getVolume,Action<float> setVolumeListener)
        {
            this.getVolume = getVolume;
            this.setVolumeListener = setVolumeListener;

            volumeSlider.onValueChanged.AddListener(SetVolume);

            float currentVolume = getVolume == null ? 0 : getVolume();

            volumeSlider.value = currentVolume;
            volumeText.text = Mathf.RoundToInt(currentVolume * 100).ToString();
        }

        public void SetActive(bool set)
        {
            if (set == false) return;
            volumeSlider.value = getVolume == null ? 0 : getVolume();

        }

        private void SetVolume(float value)
        {
            setVolumeListener?.Invoke(value);
            volumeText.text = Mathf.RoundToInt(value * 100).ToString();
        }

    }
}