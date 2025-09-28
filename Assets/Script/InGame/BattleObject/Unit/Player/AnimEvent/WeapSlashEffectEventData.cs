using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Event
{
    [CreateAssetMenu(fileName = "SlashEffectAnimationEventData", menuName = "Scriptable/AnimationEventData/SlashEffectAnimationEventData")]
    public class WeapSlashEffectEventData : AnimationEventData
    {
        [SerializeField] private Vector3 localPos;
        [SerializeField] private Vector3 localRot;

        public Vector3 LocalPos => localPos;
        public Quaternion LocalRot => Quaternion.Euler(localRot);
    }
}
