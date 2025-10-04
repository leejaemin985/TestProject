using UnityEngine;

namespace InGame.Event
{
    [CreateAssetMenu(fileName = "WalkStepSEAnimEvent", menuName = "Scriptable/AnimationEventData/WalkStepSEAnimEvent")]
    public class MovementWalkStepSEAnimEvent : AnimationEventData
    {
        public enum Direction
        {
            F,
            B,
            L,
            R,
            FL,
            FR,
            BL,
            BR
        }

        [SerializeField] private Direction direction;

        public Direction Dir => direction;
    }
}