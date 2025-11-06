using System;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace GameOption
{
    public class AudioSettingGameOptionUI : MonoBehaviour
    {
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider effectVolumeSlider;

        public Action<float> onBgmVolumeChangedListener { get; set; }
        public Action<float> onEffectVolumeChangedListener { get; set; }

        public void Initialize(Func<float> getBgmVolume, Func<float> getEffectVolume, Action<float> onBgmVolumeChangedListener, Action<float> onEffectVolumeChangedListener)
        {
            bgmVolumeSlider.value = getBgmVolume == null ? 0f : getBgmVolume.Invoke();
            effectVolumeSlider.value = getEffectVolume == null ? 0f : getEffectVolume.Invoke();

            this.onBgmVolumeChangedListener = onBgmVolumeChangedListener;
            this.onEffectVolumeChangedListener = onEffectVolumeChangedListener;

            SetListener();
        }

        private void SetListener()
        {
            bgmVolumeSlider.onValueChanged.AddListener(OnChangeBgmVolumeEvent);
            effectVolumeSlider.onValueChanged.AddListener(OnChangeEffectVolumeEvent);
        }

        private void OnChangeBgmVolumeEvent(float value) => onBgmVolumeChangedListener?.Invoke(value);
        private void OnChangeEffectVolumeEvent(float value) => onEffectVolumeChangedListener?.Invoke(value);
    }
}