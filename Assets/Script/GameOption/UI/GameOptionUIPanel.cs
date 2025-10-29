using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameOption
{
    public class GameOptionUIPanel : MonoBehaviour
    {
        [SerializeField] private Image selectedImage;

        public Action onClickedEvent { get; set; }

        public void OnClickPanelEvent() => onClickedEvent?.Invoke();


        public void SetActive(bool set)
        {
            selectedImage.enabled = set;
        }
    }
}
