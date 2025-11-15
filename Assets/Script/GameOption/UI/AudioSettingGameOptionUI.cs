using System;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace GameOption
{
    public class AudioSettingGameOptionUI : MonoBehaviour
    {
        [SerializeField] private SoundSliderSetter bgmVolumeSetter;
        [SerializeField] private SoundSliderSetter effectVolumeSetter;

        public void Initialize(Func<float> getBgmVolume, Func<float> getEffectVolume, Action<float> onBgmVolumeChangedListener, Action<float> onEffectVolumeChangedListener)
        {
            bgmVolumeSetter.Initialize(getBgmVolume, onBgmVolumeChangedListener);
            effectVolumeSetter.Initialize(getEffectVolume, onEffectVolumeChangedListener);
        }

        public void PanelSetActive(bool set) => gameObject.SetActive(set);

    }
}