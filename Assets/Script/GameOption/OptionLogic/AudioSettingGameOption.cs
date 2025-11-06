using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.Sound;

namespace GameOption
{
    public class AudioSettingGameOption : GameOptionSetterBase
    {
        [SerializeField] private AudioSettingGameOptionUI audioUI;   

        protected override string OptionName => "AudioSetting";

        protected override void Initialize()
        {
            base.Initialize();
            audioUI.Initialize(GetBgmVolume, GetEffectVolume, OnChangedBgmVolume, OnChangedEffectVolume);
        }

        private float GetBgmVolume() => GameAudioMixerController.Instance.GetBGMVolume();

        private float GetEffectVolume() => GameAudioMixerController.Instance.GetEffectVolume();


        private void OnChangedBgmVolume(float value)
        {
            GameAudioMixerController.Instance.SetBGMVolume(value);
        }

        private void OnChangedEffectVolume(float value)
        {
            GameAudioMixerController.Instance.SetEffectVolume(value);
        }

    }
}
