using System;
using UnityEngine;

namespace Utility.EffectObject
{
    interface IEffectObject
    {
        public bool GetActive();

        public void SetActive(bool set);

        public void SetParent(Transform targetParent);

        public void SetLocalPosAndRot(Vector3 pos, Quaternion rot);

        public void OnEffect(Action completeListener);

        public void OffEffect();
    }
}