using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Spinner
{
    public class Spinner : MonoSingleton<Spinner>
    {
        [SerializeField] private SpinnerUI uiHandle = default;

        private const int SPINNER_ADD_ROT = 30;
        private WaitForSeconds spinnerRotDelay = new WaitForSeconds(.1f);

        private IEnumerator spinnerRoutineHandle = default;


        public void OnSpinner(Func<bool> until)
        {
            if (spinnerRoutineHandle != null) StopCoroutine(spinnerRoutineHandle);
            StartCoroutine(spinnerRoutineHandle = SpinnerRoutine(until));
        }

        private IEnumerator SpinnerRoutine(Func<bool> until)
        {
            uiHandle.SetSpinner(true);

            while (until.Invoke() == false)
            {
                uiHandle.AddRotSpinner(SPINNER_ADD_ROT);
                yield return spinnerRotDelay;
            }

            uiHandle.SetSpinner(false);
        }
    }
}
