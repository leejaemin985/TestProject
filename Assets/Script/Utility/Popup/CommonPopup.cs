using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Utility.CommonPopup
{
    public class CommonPopup : MonoSingleton<CommonPopup>
    {
        public readonly struct PopupPolicy
        {
            public enum PopupKind
            {
                Confirm,
                YesNo
            }

            public readonly PopupKind Kind;

            public readonly string Title;
            public readonly string Description;

            public readonly string MainButtonText;
            public readonly string SubButtonText;

            public readonly Action MainButtonEvent;
            public readonly Action SubButtonEvent;

            public PopupPolicy(
                PopupKind popupKind,
                string title,
                string description,
                string mainButtonText,
                string subButtonText = null,
                Action mainButtonEvent = null,
                Action subButtonEvent = null)
            {
                this.Kind = popupKind;

                this.Title = title;
                this.Description = description;

                this.MainButtonText = mainButtonText;
                this.SubButtonText = subButtonText;

                this.MainButtonEvent = mainButtonEvent;
                this.SubButtonEvent = subButtonEvent;
            }
        }

        [SerializeField] private RectTransform rect;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private Image subButtonImage;

        [SerializeField] private TMP_Text mainButtonText;
        [SerializeField] private TMP_Text subButtonText;

        private Action onClickedMainButtonListener;
        private Action onClickedSubButtonListener;

        public void OnPopup(PopupPolicy popupPolicy)
        {
            subButtonImage.gameObject.SetActive(popupPolicy.Kind == PopupPolicy.PopupKind.YesNo);

            rect.gameObject.SetActive(true);

            titleText.text = popupPolicy.Title;
            contentText.text = popupPolicy.Description;

            mainButtonText.text = popupPolicy.MainButtonText;
            subButtonText.text = popupPolicy.SubButtonText;
            onClickedMainButtonListener = popupPolicy.MainButtonEvent;
            onClickedSubButtonListener = popupPolicy.SubButtonEvent;
        }

        public void OffPopup()
        {
            rect.gameObject.SetActive(false);
        }

        public void OnClickedMainButtonEvent()
        {
            rect.gameObject.SetActive(false);
            onClickedMainButtonListener?.Invoke();
        }

        public void OnClickedSubButtonEvent()
        {
            rect.gameObject.SetActive(false);
            onClickedSubButtonListener?.Invoke();
        }
    }
}
