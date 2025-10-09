using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Spinner
{
    public class Spinner : MonoSingleton<Spinner>
    {
        [SerializeField] private SpinnerUI uiHandle;
        private const string LOADING_IMAGE_PATH = "Spinner/image/LoadingImage_1";

        private const int SPINNER_ADD_ROT = 30;
        private WaitForSeconds spinnerRotDelay = new WaitForSeconds(.1f);

        private IEnumerator spinnerRoutineHandle;

        public void OnSpinner(Func<bool> until, bool onLoadingImage = false, string text = null)
        {
            uiHandle.SetSpinner(true, onLoadingImage);
            SetText(text);

            if (spinnerRoutineHandle != null) StopCoroutine(spinnerRoutineHandle);
            StartCoroutine(spinnerRoutineHandle = SpinnerRoutine(until));
        }

        public void OffSpinner()
        {
            uiHandle.SetSpinner(false, false);
        }

        public void SetText(string text) => uiHandle.SetText(text);

        private IEnumerator SpinnerRoutine(Func<bool> until)
        {
            while (until.Invoke() == false)
            {
                uiHandle.AddRotSpinner(SPINNER_ADD_ROT);
                yield return spinnerRotDelay;
            }

            uiHandle.SetSpinner(false, false);
        }
    }
}
