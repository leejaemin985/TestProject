using InGame.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Event
{
    [CreateAssetMenu(fileName = "WeapSoundEffectEventData", menuName = "Scriptable/AnimationEventData/WeapSoundEffectEventData")]
    public class WeapSoundEffectEventData : AnimationEventData
    {
        [SerializeField] private WeapSoundObject.WeapSoundType weapSoundType;
        [SerializeField] private int presetOrder = -1;

        public WeapSoundObject.WeapSoundType WeapSoundType => weapSoundType;
        public int PresetOrder => presetOrder;
    }
}
