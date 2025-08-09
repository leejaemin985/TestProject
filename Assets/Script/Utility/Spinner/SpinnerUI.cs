using UnityEngine;
using UnityEngine.UI;

namespace Utility.Spinner
{
    public class SpinnerUI : MonoBehaviour
    {
        [SerializeField] private Image spinnerImage = default;
        [SerializeField] private Image blockingPanel = default;

        public void SetSpinner(bool set)
        {
            spinnerImage.gameObject.SetActive(set);
            blockingPanel.gameObject.SetActive(set);
        }

        public void AddRotSpinner(float addZRot)
        {
            Vector3 currentRot = spinnerImage.rectTransform.eulerAngles;
            currentRot.z += addZRot;
            spinnerImage.rectTransform.rotation = Quaternion.Euler(currentRot);
        }

    }
}