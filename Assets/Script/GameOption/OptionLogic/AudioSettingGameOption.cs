using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility.Sound;

namespace GameOption
{
    public class AudioSettingGameOption : GameOptionSetterBase
    {
        private class AudioSettingData
        {
            public float bgmVolume;
            public float effectVolume;
        }

        [SerializeField] private AudioSettingGameOptionUI audioUI;

        private string FilePath => Path.Combine(Application.persistentDataPath, "AudioSettingData.json");

        protected override GameOptionType OptionType => GameOptionType.Audio;

        protected override void Initialize()
        {
            base.Initialize();
            audioUI.Initialize(GetBgmVolume, GetEffectVolume, OnChangedBgmVolume, OnChangedEffectVolume);
        }

        protected override void SetActive(bool set)
        {
            audioUI.PanelSetActive(set);
        }

        #region Load/Save File
        private void SaveAudioSettingData(AudioSettingData data)
        {
            if (data == null) return;

            string json = JsonUtility.ToJson(data, prettyPrint: true);

            try
            {
                File.WriteAllText(FilePath, json);

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

        }

        private AudioSettingData LoadAudioSettingData()
        {
            AudioSettingData GetNewData()
            {
                return new()
                {
                    bgmVolume = 1,
                    effectVolume = 1
                };
            }

            if (File.Exists(FilePath) == false)
                return GetNewData();

            AudioSettingData ret = null;
            try
            {
                string json = File.ReadAllText(FilePath);
                ret = JsonUtility.FromJson<AudioSettingData>(json);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            return ret == null ? GetNewData() : ret;
        }
        #endregion

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
