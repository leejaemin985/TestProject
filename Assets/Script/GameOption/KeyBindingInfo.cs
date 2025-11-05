using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOption
{
    public class KeyBindingInfo
    {
        public readonly InputAction inputAction;
        public readonly int bindingIndex;

        public InputBinding inputBinding => inputAction.bindings[bindingIndex];

        public KeyBindingInfo(InputAction sourceAction, int index)
        {
            inputAction = sourceAction;
            bindingIndex = index;
            DisplayName = string.IsNullOrEmpty(inputBinding.name) ? inputBinding.action : inputBinding.name;
        }

        public string CurrentKeyPath => string.IsNullOrEmpty(inputBinding.overridePath) ? inputBinding.path : inputBinding.overridePath;
        public readonly string DisplayName;
    }

}