using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.EffectObject
{
    public abstract class EffectObject : MonoBehaviour, IEffectObject
    {
        protected virtual bool GetActive() { return gameObject.activeSelf; }

        protected virtual void SetActive(bool set) { gameObject.SetActive(set); }

        protected virtual void OnEffect(Action completeListener) { }

        protected virtual void OffEffect() { }

        #region IEffectObject
        bool IEffectObject.GetActive() => GetActive();

        void IEffectObject.SetActive(bool set) => SetActive(set);

        void IEffectObject.SetParent(Transform targetParent) => transform.SetParent(targetParent);

        void IEffectObject.SetLocalPosAndRot(Vector3 pos, Quaternion rot) => transform.SetLocalPositionAndRotation(pos, rot);

        void IEffectObject.OnEffect(Action completeListener) => OnEffect(completeListener);

        void IEffectObject.OffEffect() => OffEffect();
        #endregion
    }
}
