using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


namespace GameOption
{
    public class InputSystemSettings
    {
        private const string DEFAULT_ASSET_PATH = "InputSystem/GameInputActions";

        private const string KEY_INFO_BASE_PATH = "KeyInfos.json";
        private static string KeyInfoPath => Path.Combine(Application.persistentDataPath, KEY_INFO_BASE_PATH);

        private static void LoadKeyInfoFile(InputActionAsset inputAction)
        {
            if (File.Exists(KeyInfoPath) == false)
            {
                Debug.LogWarning($"Not Found KeyInfo File (path: {KeyInfoPath})");
                return;
            }

            try
            {
                string loadedJson = File.ReadAllText(KeyInfoPath);
                inputAction.LoadBindingOverridesFromJson(loadedJson);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static void SaveKeyInfoFile(InputActionAsset inputAction)
        {
            try
            {
                string keyInfoJson = inputAction.SaveBindingOverridesAsJson();
                File.WriteAllText(KeyInfoPath, keyInfoJson);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }


        private InputActionAsset inputActions;
        private List<KeyBindingInfo> keyBindingInfos;

        public void Initialize()
        {
            inputActions = Resources.Load<InputActionAsset>(DEFAULT_ASSET_PATH);
            if (inputActions == null)
            {
                Debug.LogError($"Not Found InputActionAsset");
                return;
            }

            //Load InputAction Overriding
            LoadKeyInfoFile(inputActions);

            keyBindingInfos = new();
            foreach(var action in inputActions)
            {
                for (int index = 0; index < action.bindings.Count; ++index)
                {
                    var binding = action.bindings[index];
                    if (binding.isComposite) continue;

                    keyBindingInfos.Add(new(binding, action, index));
                }
            }
        }

        public List<KeyBindingData> GetKeyBindingInfos()
        {
            return keyBindingInfos.Select(info => new KeyBindingData(info)).ToList();
        }

        public void SetKeyBinding(List<KeyBindingData> bindingDatas)
        {
            foreach (var requestBind in bindingDatas)
            {
                if (requestBind.IsModified == false) continue;

                keyBindingInfos.Find(x => x.DisplayName.Equals(requestBind.DisPlayName)).TryChangeKeyPath(requestBind);
            }

            //Save InputActionAsset Overrding
            SaveKeyInfoFile(inputActions);
        }
    }
}