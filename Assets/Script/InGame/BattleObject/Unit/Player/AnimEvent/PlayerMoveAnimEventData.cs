using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Event
{
    [CreateAssetMenu(fileName ="PlayerMoveAnimEventData", menuName ="Scriptable/AnimationEventData/PlayerMoveAnimEventData")]
    public class PlayerMoveAnimEventData : AnimationEventData
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private Vector3 moveDir;

        public float MoveSpeed => moveSpeed;
        public Vector3 MoveDir => moveDir;
    }
}
