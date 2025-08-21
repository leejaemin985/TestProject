using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Utility.CommonPopup
{
    public class CommonPopup : MonoSingleton<CommonPopup>
    {
        [SerializeField] private RectTransform rect;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private Image mainButtonImage;
        [SerializeField] private Image subButtonImage;

        [SerializeField] private TMP_Text mainButtonText;
        [SerializeField] private TMP_Text subButtonText;

        private Action onClickedMainButtonListener;
        private Action onClickedSubButtonListener;


        public void OnConfirmCommonPopup(string title, string content, string confirmText, Action confirmAction = null)
        {
            rect.gameObject.SetActive(true);
            mainButtonImage.gameObject.SetActive(true);
            subButtonImage.gameObject.SetActive(false);

            titleText.text = title;
            contentText.text = content;

            mainButtonText.text = confirmText;
            onClickedMainButtonListener = confirmAction;
        }

        public void OnYesNoCommonPopup(string title, string content, string yesText, string noText, Action yesAction = null, Action noAction = null)
        {
            rect.gameObject.SetActive(true);
            mainButtonImage.gameObject.SetActive(true);
            subButtonImage.gameObject.SetActive(true);

            titleText.text = title;
            contentText.text = content;

            mainButtonText.text = yesText;
            subButtonText.text = noText;
            onClickedMainButtonListener = yesAction;
            onClickedSubButtonListener = noAction;
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
