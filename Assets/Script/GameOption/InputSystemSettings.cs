using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOption
{
    public class InputSystemSettings : MonoSingleton<InputSystemSettings>
    {
        [SerializeField] private InputActionAsset inputActions;
        private List<KeyBindingInfo> keyBindingInfos;

        private const string KEY_INFO_BASE_PATH = "KeyInfos.json";
        private string KeyInfoPath => Path.Combine(Application.persistentDataPath, KEY_INFO_BASE_PATH);

        /*
        public void LoadKeyInfoFile(InputActionAsset inputAction, Action completeAction = null)
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
            finally
            {
                completeAction?.Invoke();
            }
        }

        public void SaveKeyInfoFile(InputActionAsset inputAction, Action completeAction = null)
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
            finally
            {
                completeAction?.Invoke();
            }
        }
        */

        #region TestCode

        private void Start()
        {
            Initialize(inputActions);
        }

        private void Update()
        {
            if (Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                foreach (var binding in inputActions.bindings)
                {
                    if (binding.isComposite) continue;

                    string name = string.IsNullOrEmpty(binding.name) ? binding.action : binding.name;
                    Debug.Log($"[{name}] - path: {binding.path} // {binding.overridePath}");
                }
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                var dataList = GetKeyBindingInfos();
                dataList[0].CurrentKeyPath = "<Keyboard>/p";

                SetKeyBinding(dataList);
            }
        }

        #endregion

        public void Initialize(InputActionAsset inputActions)
        {
            if (inputActions == null) return;

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
            //bool modified = false;

            foreach (var requestBind in bindingDatas)
            {
                if (requestBind.IsModified == false) continue;

                //modified |=
                keyBindingInfos.Find(x => x.DisplayName.Equals(requestBind.DisPlayName)).TryChangeKeyPath(requestBind);
            }
            Debug.Log($"Test - SetKeyBinding Done");
            //if (modified == false) return;
        }

    }
}