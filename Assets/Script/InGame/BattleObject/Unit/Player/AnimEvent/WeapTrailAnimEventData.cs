using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Event
{
    [CreateAssetMenu(fileName = "WeapTrailAnimEventData", menuName = "Scriptable/AnimationEventData/WeapTrailAnimEventData")]
    public class WeapTrailAnimEventData : AnimationEventData
    {
        [SerializeField] private bool setTrail;

        public bool SetTrail => setTrail;
    }
}
