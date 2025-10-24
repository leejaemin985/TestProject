using ExitGames.Client.Photon.StructWrapping;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTest : MonoBehaviour
{
    //[SerializeField] private InputActionAsset inputActions;



    public void OnMove(InputAction.CallbackContext context)
    {
        var moveDir = context.ReadValue<Vector2>();
        if (moveDir == null) return;

        Debug.Log($"Test - moveDir: {moveDir}");
    }


    /*
    private const string KEY_INFO_BASE_PATH = "KeyInfos.json";
    private string KeyInfoPath => Path.Combine(Application.persistentDataPath, KEY_INFO_BASE_PATH);

    private class KeyInfo
    {
        public string name;
        public string path;

        public KeyInfo(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }

    private Dictionary<string, KeyInfo> keySettingMap = new()
        {
            //MoveDir
            { "up", new KeyInfo("up", null)},
            { "down", new KeyInfo("down", null)},
            { "left", new KeyInfo("left", null)},
            { "right", new KeyInfo("right", null)},

            { "Dash", new KeyInfo("Dash", null)},
            { "Jump", new KeyInfo("Jump", null)},
            { "Attack", new KeyInfo("Attack", null)},
            { "Defense", new KeyInfo("Defense", null)},
            { "Skill", new KeyInfo("Skill", null)}
        };


    private void Start()
    {
        InitKeySettingMap();
    }

    public void Update()
    {
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            PrintKeySetting();
        }


        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            var jsonFile = inputActions.SaveBindingOverridesAsJson();

            File.WriteAllText(KeyInfoPath, jsonFile);
            Debug.Log($"Test - Save KeyInfo");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            string jsonFile = File.ReadAllText(KeyInfoPath);
            inputActions.LoadBindingOverridesFromJson(jsonFile);
            Debug.Log($"Test - Load KeyInfo");
        }


        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            var action = inputActions.FindAction("Jump", true);
            action.ApplyBindingOverride("<Keyboard>/k");
        }
    }

    private void InitKeySettingMap()
    {
        foreach (var bind in inputActions.bindings)
        {
            if (keySettingMap.TryGetValue(bind.name, out KeyInfo compositeKeyInfo))
            {
                compositeKeyInfo.path = bind.path;
            }
            else if (keySettingMap.TryGetValue(bind.action, out KeyInfo keyInfo))
            {
                keyInfo.path = bind.path;
            }
        }

        inputActions.SaveBindingOverridesAsJson();
    }

    private void PrintKeySetting()
    {
        Debug.Log($"Test - Called Print KeySetting");
        foreach (var bind in inputActions.bindings)
        {
            if (bind.isComposite) continue;
            string name = string.IsNullOrEmpty(bind.name) ? bind.action : bind.name;
            Debug.Log($"[{name}] - path: {bind.path} // {bind.overridePath}");
        }
    }
    */
}
