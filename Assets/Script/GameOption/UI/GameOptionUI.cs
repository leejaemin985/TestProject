using System;
using UnityEngine;

namespace GameOption
{
    public class GameOptionUI : MonoBehaviour
    {
        public Action onClickKeySettingPanelListener { get; set; }
        public Action onClickAudioSettingPanelListener { get; set; }

        public bool GetActive() => gameObject.activeSelf;

        public void SetActive(bool set) => gameObject.SetActive(set);


        public void OnClickKeySettingPanelEvent()
        {
            onClickKeySettingPanelListener?.Invoke();
        }

        public void OnClickAudioSettingPanelEvent()
        {
            onClickAudioSettingPanelListener?.Invoke();
        }
    }
}