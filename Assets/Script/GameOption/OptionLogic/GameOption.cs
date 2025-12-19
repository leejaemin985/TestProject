using Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.CommonPopup;
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

        public bool GetPanelActive() => optionUI.GetActive();

        public void SetPanelActive(bool set) => optionUI.SetActive(set);

        public void OnExitPopup()
        {
            CommonPopup.Instance.OnPopup(new
            (
                popupKind: CommonPopup.PopupPolicy.PopupKind.YesNo,
                title: "Exit the game",
                description: null,
                mainButtonText: "Yes",
                subButtonText: "No",
                mainButtonEvent: Application.Quit,
                subButtonEvent: null
            ));
        }
    }
}