using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.EffectObject
{
    public class EffectObjectPool : MonoBehaviour
    {
        public struct PoolingInfo
        {
            public int count;
            public Transform effectRoot;
        }

        private const int POOL_MIN_SIZE = 5;

        public static EffectObjectPool CreatePoolInstance<T>(T effectObject, PoolingInfo poolingInfo) where T : EffectObject
        {
            if (poolingInfo.count < POOL_MIN_SIZE) poolingInfo.count = POOL_MIN_SIZE;

            var ret = new GameObject($"EffectObject<{effectObject.GetType()}>_Pool").AddComponent<EffectObjectPool>();
            ret.Initialize(effectObject, poolingInfo);

            return ret;
        }


        private int currentIndex;
        private Transform effectRoot;
        private IEffectObject[] effectObjects;

        private void Initialize<T>(T effectSource, PoolingInfo poolingInfo) where T : EffectObject
        {
            effectObjects = new IEffectObject[poolingInfo.count];
            effectRoot = poolingInfo.effectRoot;

            for (int index = 0, max = poolingInfo.count; index < max; ++index)
            {
                var effectInstace = Instantiate(effectSource);
                effectInstace.transform.SetParent(transform);

                effectObjects[index] = effectInstace;
                effectObjects[index].SetActive(false);
            }
        }

        public void OnPlayEffect(Vector3 localPos, Quaternion localRot)
        {
            var currentEffect = effectObjects[currentIndex];
            currentIndex++;
            currentIndex = currentIndex % effectObjects.Length;

            currentEffect.SetActive(true);
            currentEffect.SetParent(effectRoot);
            currentEffect.SetLocalPosAndRot(localPos, localRot);

            currentEffect.OnEffect(()=>
            {
                currentEffect.SetActive(false);
                currentEffect.SetParent(transform);
            });
        }
    }
}
