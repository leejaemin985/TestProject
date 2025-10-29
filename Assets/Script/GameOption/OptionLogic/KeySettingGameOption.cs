using UnityEngine;

namespace GameOption
{
    public class KeySettingGameOption : GameOptionSetterBase
    {
        protected override string OptionName => "KeySetting";

        [SerializeField] private KeySettingGameOptionUI keySettingUI;
        [SerializeField] private name
        
        private float mouseSensitive { get; set; }

        private InputSystemSettings inputSystemSetter;

        protected override void Initialize()
        {
            base.Initialize();
            inputSystemSetter.Initialize();
        }

        protected override void SetActive(bool set)
        {

        }

    }
}