using UnityEngine;
using UnityEngine.UI;

namespace Utility.Spinner
{
    public class SpinnerUI : MonoBehaviour
    {
        [SerializeField] private Image spinnerImage;
        [SerializeField] private Image blockingPanel;
        [SerializeField] private Image backgroundLetterBox;
        [SerializeField] private Image backgroundImage;

        public void SetSpinner(bool set, bool loadingImage)
        {
            spinnerImage.gameObject.SetActive(set);
            blockingPanel.gameObject.SetActive(set);

            backgroundLetterBox.gameObject.SetActive(loadingImage);
        }

        //public void SetLoadingImage(Sprite sprite)
        //{
        //    backgroundImage.sprite = sprite;
        //}

        public void AddRotSpinner(float addZRot)
        {
            Vector3 currentRot = spinnerImage.rectTransform.eulerAngles;
            currentRot.z += addZRot;
            spinnerImage.rectTransform.rotation = Quaternion.Euler(currentRot);
        }

    }
}