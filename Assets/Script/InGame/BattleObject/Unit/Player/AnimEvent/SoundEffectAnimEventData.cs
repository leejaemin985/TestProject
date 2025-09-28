using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Event
{
    [CreateAssetMenu(fileName = "SoundEffectAnimEventData", menuName = "Scriptable/AnimationEventData/SoundEffectAnimEventData")]
    public class SoundEffectAnimEventData : AnimationEventData
    {
        [SerializeField] private int presetOrder;

        public int PresetOrder => presetOrder;
    }
}
