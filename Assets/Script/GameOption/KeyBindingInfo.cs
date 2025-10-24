using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOption
{
    public class KeyBindingInfo
    {
        private InputAction inputAction;
        private int bindingIndex;

        public InputBinding inputBinding { get; private set; }


        public bool TryChangeKeyPath(KeyBindingData keyBindingData)
        {
            if (DisplayName.Equals(keyBindingData.DisPlayName) == false) return false;

            inputAction.ApplyBindingOverride(bindingIndex, keyBindingData.CurrentKeyPath);
            CurrentKeyPath = keyBindingData.CurrentKeyPath;
            return true;
        }

        public KeyBindingInfo(InputBinding sourceBinding, InputAction sourceAction, int index)
        {
            if (sourceBinding.isComposite)
            {
                Debug.LogError("InputBinding.IsComposite cannot be assigned to KeyBindingInfo.");
                return;
            }

            inputAction = sourceAction;
            bindingIndex = index;
            inputBinding = sourceBinding;
            CurrentKeyPath = string.IsNullOrEmpty(inputBinding.overridePath) ? inputBinding.path : inputBinding.overridePath;
            DisplayName = string.IsNullOrEmpty(inputBinding.name) ? inputBinding.action : inputBinding.name;
        }

        public string CurrentKeyPath { get; private set; }
        public readonly string DisplayName;
    }

}