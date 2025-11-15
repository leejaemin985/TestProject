using System;
using TMPro;
using UnityEngine;

namespace GameOption
{
    public class KeySettingGameOptionUI : MonoBehaviour
    {
        [SerializeField] private GameObject uiObject;


        public void PanelSetActive(bool set) => uiObject.SetActive(set);
    }
}