using System.Collections.Generic;
using UnityEngine;
using Utility.Sound;

namespace InGame.Weapon
{
    public class WeapSoundObject : GameEffectSoundObject
    {
        public enum WeapSoundType
        {
            Whoosh,
            Collision,
            Custom
        }

        private Dictionary<WeapSoundType, List<AudioClip>> clipListMap;

        internal void SetClipListMap(Dictionary<WeapSoundType, List<AudioClip>> clipListMap)
        {
            this.clipListMap = clipListMap;
        }

        internal void PlayOneShotWeapSE(WeapSoundType weapSoundType, int presetOrder = -1)
        {
            if (clipListMap.ContainsKey(weapSoundType) == false)
            {
                Debug.LogError($"In Game - Not Found WeaponSE Key Exception(key: {weapSoundType})");
                return;
            }

            var clipList = clipListMap[weapSoundType];
            if (presetOrder < 0)
                presetOrder = Random.Range(0, clipList.Count);

            PlayOneShot(clipList[presetOrder % clipList.Count]);
        }
    }
}