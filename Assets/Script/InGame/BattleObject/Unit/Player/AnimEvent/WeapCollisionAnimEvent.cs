using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Event
{
    [CreateAssetMenu(fileName = "WeapCollisionAnimEventData", menuName = "Scriptable/AnimationEventData/WeapCollisionAnimEventData")]
    public class WeapCollisionAnimEvent : AnimationEventData
    {
        [SerializeField] private bool setCollision;

        public bool SetCollision => setCollision;
    }
}
