using Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.Sound;

namespace GameOption
{
    public class GameOption : MonoSingleton<GameOption>
    {
        [SerializeField] private GameOptionUI optionUI;

        [SerializeField] private GameOptionSetterBase[] setterBases;

        private Dictionary<GameOptionSetterBase.GameOptionType, IGameOptionSetter> optionsMap;

        public void Initialize()
        {
            optionsMap = new();
            foreach (var setter in setterBases)
            {
                var optionSetter = setter as IGameOptionSetter;

                optionsMap.Add(optionSetter.GameOptionType, optionSetter);
                optionSetter.Initialize();
            }

            SetListener();
        }

        private void SetListener()
        {
            optionUI.onClickKeySettingPanelListener = OpenKeySetting;
            optionUI.onClickAudioSettingPanelListener = OpenAudioSetting;
        }

        private void CloseAllSetter()
        {
            foreach (var setter in optionsMap.Values) setter.SetActive(false);
        }

        private void OpenKeySetting()
        {
            CloseAllSetter();
            optionsMap[GameOptionSetterBase.GameOptionType.KeySetting].SetActive(true);
        }

        private void OpenAudioSetting()
        {
            CloseAllSetter();
            optionsMap[GameOptionSetterBase.GameOptionType.Audio].SetActive(true);
        }

        private void Update()
        {
            //if (Keyboard.current.escapeKey.wasPressedThisFrame)
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                bool targetActive = !optionUI.GetActive();
                optionUI.SetActive(targetActive);
            }
        }

    }
}