using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOption
{
    public class InputSystemSettings : MonoSingleton<InputSystemSettings>
    {
        [SerializeField] private InputActionAsset inputActions;

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

        private void InitKeySettingMap()
        {
            foreach (var bind in inputActions.bindings)
            {
                Debug.Log($"[{bind.name}] - path: {bind.path}");
                if (keySettingMap.TryGetValue(bind.name, out KeyInfo keyInfo))
                {
                    keyInfo.path = bind.path;
                }
            }

            Debug.Log($"Init KeySettingMap Done");
        }
    }
}