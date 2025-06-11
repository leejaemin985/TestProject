using UnityEngine;

namespace Physics
{
    public struct OBB
    {
        public Vector3 center;
        public Vector3[] axis;
        public Vector3 halfSize;

        private Vector3[] _cachedVertices;

        public OBB(Transform boxTransform)
        {
            center = boxTransform.position;
            axis = new Vector3[3];
            axis[0] = boxTransform.right.normalized;
            axis[1] = boxTransform.up.normalized;
            axis[2] = boxTransform.forward.normalized;
            halfSize = boxTransform.lossyScale * 0.5f;

            _cachedVertices = new Vector3[8];
            UpdateVertices();
        }

        public OBB(Vector3 center, Vector3 eulerRotation, Vector3 size)
        {
            this.center = center;

            Quaternion rotation = Quaternion.Euler(eulerRotation);
            axis = new Vector3[3];
            axis[0] = rotation * Vector3.right;
            axis[1] = rotation * Vector3.up;
            axis[2] = rotation * Vector3.forward;

            halfSize = size * 0.5f;

            _cachedVertices = new Vector3[8];
            UpdateVertices();
        }

        private void UpdateVertices()
        {
            _cachedVertices[0] = center + axis[0] * halfSize.x + axis[1] * halfSize.y + axis[2] * halfSize.z;
            _cachedVertices[1] = center + axis[0] * halfSize.x + axis[1] * halfSize.y - axis[2] * halfSize.z;
            _cachedVertices[2] = center + axis[0] * halfSize.x - axis[1] * halfSize.y + axis[2] * halfSize.z;
            _cachedVertices[3] = center + axis[0] * halfSize.x - axis[1] * halfSize.y - axis[2] * halfSize.z;
            _cachedVertices[4] = center - axis[0] * halfSize.x + axis[1] * halfSize.y + axis[2] * halfSize.z;
            _cachedVertices[5] = center - axis[0] * halfSize.x + axis[1] * halfSize.y - axis[2] * halfSize.z;
            _cachedVertices[6] = center - axis[0] * halfSize.x - axis[1] * halfSize.y + axis[2] * halfSize.z;
            _cachedVertices[7] = center - axis[0] * halfSize.x - axis[1] * halfSize.y - axis[2] * halfSize.z;
        }

        public Vector3[] GetVertices()
        {
            UpdateVertices();
            return _cachedVertices;
        }
    }

    public abstract class PhysicsObject : MonoBehaviour
    {
        public enum PhysicsType
        {
            ATTACK,
            HITABLE,
            ALL
        }

        public abstract PhysicsType physicsType { get; }

        private void Start() => Initialized();

        protected virtual void Initialized()
        {
            PhysicsGenerator.Instance.RegisterPhysicsObject(this);
        }
    }
}