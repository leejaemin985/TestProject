using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;

namespace Physics
{
    public interface IPhysicsShape
    {
        PhysicsObject.PhysicsShapeType ShapeType { get; }

        Type CollisionType { get; }

        IPhysicsShape ComputeSweptVolume(IPhysicsShape next);

        void UpdateFromTransform(Transform transform);
    }

    public struct Sphere : IPhysicsShape
    {
        public PhysicsObject.PhysicsShapeType ShapeType => PhysicsObject.PhysicsShapeType.SPHERE;
        public Type CollisionType => typeof(Sphere);

        public IPhysicsShape ComputeSweptVolume(IPhysicsShape next)
        {
            if (next is not Sphere other)
            {
                Debug.LogError("[Physics] - Shape types must match for swept volume calcuation.");
                return null;
            }

            return SweptVolumeCalculator.ComputeSweptSphere(this, other);
        }

        public float3 center;
        public float radius;

        public Sphere(Transform transform)
        {
            this.center = default;
            this.radius = default;
            UpdateFromTransform(transform);
        }

        public Sphere(float3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public bool ContainsPoint(float3 point)
        {
            return math.lengthsq(point - center) <= radius * radius;
        }

        public void UpdateFromTransform(Transform transform)
        {
            center = transform.position;
            float3 scale = transform.lossyScale;
            radius = math.max(math.max(scale.x, scale.y), scale.z) * 0.5f;
        }
    }

    public struct OBB : IPhysicsShape
    {
        public PhysicsObject.PhysicsShapeType ShapeType => PhysicsObject.PhysicsShapeType.OBB;
        public Type CollisionType => typeof(OBB);

        public IPhysicsShape ComputeSweptVolume(IPhysicsShape next)
        {
            if (next is not OBB other)
            {
                Debug.LogError("[Physics] - Shape types must match for swept volume calcuation.");
                return null;
            }

            return SweptVolumeCalculator.ComputeEncompassingOBB(this, other);
        }

        public float3 center;
        public float3[] axis;
        public float3 halfSize;

        private float3[] _cachedVertices;

        public OBB(Transform boxTransform)
        {
            this.center = default;
            this.axis = default;
            this.halfSize = default;
            this._cachedVertices = default;

            UpdateFromTransform(boxTransform);
        }

        public OBB(float3 center, float3 eulerRotation, float3 size)
        {
            this.center = center;

            quaternion rotation = quaternion.EulerXYZ(math.radians(eulerRotation));
            axis = new float3[3];
            axis[0] = math.mul(rotation, new float3(1, 0, 0));
            axis[1] = math.mul(rotation, new float3(0, 1, 0));
            axis[2] = math.mul(rotation, new float3(0, 0, 1));

            halfSize = size * 0.5f;

            _cachedVertices = new float3[8];
            UpdateVertices();
        }

        public OBB(float3 center, float3[] axis, float3 halfSize)
        {
            this.center = center;
            this.axis = new float3[3];
            this.axis[0] = math.normalize(axis[0]);
            this.axis[1] = math.normalize(axis[1]);
            this.axis[2] = math.normalize(axis[2]);
            this.halfSize = halfSize;

            _cachedVertices = new float3[8];
            UpdateVertices();
        }

        public void UpdateVertices()
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

        public float3[] GetVertices()
        {
            if (_cachedVertices == null) _cachedVertices = new float3[8];
            UpdateVertices();
            return _cachedVertices;
        }

        public void UpdateFromTransform(Transform transform)
        {
            center = transform.position;

            if (axis == null || axis.Length != 3)
                axis = new float3[3];

            axis[0] = math.normalize(transform.right);
            axis[1] = math.normalize(transform.up);
            axis[2] = math.normalize(transform.forward);
            halfSize = transform.lossyScale * 0.5f;

            if (_cachedVertices == null || _cachedVertices.Length != 8)
                _cachedVertices = new float3[8];

            UpdateVertices();
        }
    }

    public struct Capsule : IPhysicsShape
    {
        public PhysicsObject.PhysicsShapeType ShapeType => PhysicsObject.PhysicsShapeType.CAPSULE;
        public Type CollisionType => typeof(Capsule);

        public IPhysicsShape ComputeSweptVolume(IPhysicsShape next)
        {
            if (next is not Capsule other)
            {
                Debug.LogError("[Physics] - Shape types must match for swept volume calcuation.");
                return null;
            }

            return SweptVolumeCalculator.ComputeSweptOBBFromCapsules(this, other);
        }

        public float3 pointA;
        public float3 pointB;
        public float radius;

        public Capsule(float3 pointA, float3 pointB, float radius)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.radius = radius;
        }

        public Capsule(Transform transform)
        {
            this.pointA = default;
            this.pointB = default;
            this.radius = default;
            UpdateFromTransform(transform);
        }

        public float3 center => (pointA + pointB) * 0.5f;

        public float Height => math.distance(pointA, pointB) + radius * 2;

        public float3 Direction => math.normalize(pointB - pointA);

        public void UpdateFromTransform(Transform transform)
        {
            float3 center = transform.position;

            float3 up = transform.up;
            float height = transform.lossyScale.y;
            float radius = math.max(transform.lossyScale.x, transform.lossyScale.z) * 0.5f;

            float halfSegment = math.max(0f, (height * 0.5f) - radius);

            this.pointA = center + up * halfSegment;
            this.pointB = center - up * halfSegment;
            this.radius = radius;
        }
    }

    public abstract class PhysicsObject : MonoBehaviour
    {
        public Guid uid { get; private set; } = Guid.NewGuid();

        public bool active;

        public HashSet<Guid> collisionCheckedUIDs { get; private set; }

        public Dictionary<Guid, CollisionInfo> collisionCheckedInfo { get; private set; }

        public enum PhysicsShapeType
        {
            OBB,
            SPHERE,
            CAPSULE
        }

        public enum PhysicsType
        {
            ATTACK,
            HITABLE,
            HYBRID
        }

        public abstract PhysicsType physicsType { get; }

        public PhysicsShapeType physicsShapeType;
        public IPhysicsShape physicsShape { get; private set; }

        private Vector3 prevPosition;
        private Quaternion prevRotation;
        private Vector3 prevScale;

        private PhysicsShapeType prevShapeType;

        private void Start() => Initialized();

        protected virtual void Initialized()
        {
            physicsShape = CreateShape(physicsShapeType);
            PhysicsGenerator.Instance.RegisterPhysicsObject(this);
        }

        private IPhysicsShape CreateShape(PhysicsShapeType physicsShapeType)
        {
            return physicsShapeType switch
            {
                PhysicsShapeType.SPHERE => new Sphere(transform),
                PhysicsShapeType.OBB => new OBB(transform),
                PhysicsShapeType.CAPSULE => new Capsule(transform)
            };
        }

        private void CachedTransform()
        {
            this.prevPosition = transform.position;
            this.prevRotation = transform.rotation;
            this.prevScale = transform.localScale;
        }

        private bool HasTransformChanged()
        {
            return transform.position != prevPosition ||
                transform.rotation != prevRotation ||
                transform.localScale != prevScale;
        }

        public float3 GetPrevPosition() => prevPosition;

        public float3 GetCurrPosition() => transform.position;

        #region Gizmo
        private void OnDrawGizmos()
        {
            if (!PhysicsGizmoToggleWindow.IsShowingGizmos()) return;

            bool shapeChanged = physicsShape == null || (physicsShapeType != prevShapeType);
            bool transformChanged = HasTransformChanged();

            if (shapeChanged)
            {
                physicsShape = CreateShape(physicsShapeType);
                prevShapeType = physicsShapeType;

                CachedTransform();
                physicsShape.UpdateFromTransform(transform);
            }
            else if (transformChanged)
            {
                CachedTransform();
                physicsShape.UpdateFromTransform(transform);
            }

            PhysicsGizmoDrawer.OnDrawGizmoPhysicsShape(physicsShape);
        }

        #endregion
    }

    public class PhysicsGizmoToggleWindow : EditorWindow
    {
        private static bool showPhysicsGizmos = true;
        private static Color physicsColor = Color.cyan;

        private const string ShowGizmoKey = "ShowPhysicsGizmos";
        private const string GizmoColorR = "PhysicsGizmoColorR";
        private const string GizmoColorG = "PhysicsGizmoColorG";
        private const string GizmoColorB = "PhysicsGizmoColorB";
        private const string GizmoColorA = "PhysicsGizmoColorA";

        [MenuItem("Tools/Physics Gizmo Toggle")]
        public static void ShowWindow()
        {
            GetWindow<PhysicsGizmoToggleWindow>("Physics Gizmo Toggle");
        }

        private void OnEnable()
        {
            LoadPrefs();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            showPhysicsGizmos = EditorGUILayout.Toggle("Show Physics Gizmos", showPhysicsGizmos);
            physicsColor = EditorGUILayout.ColorField("Physics Range Color", physicsColor);

            if (EditorGUI.EndChangeCheck())
            {
                SavePrefs();
            }
        }

        private static void SavePrefs()
        {
            EditorPrefs.SetBool(ShowGizmoKey, showPhysicsGizmos);
            EditorPrefs.SetFloat(GizmoColorR, physicsColor.r);
            EditorPrefs.SetFloat(GizmoColorG, physicsColor.g);
            EditorPrefs.SetFloat(GizmoColorB, physicsColor.b);
            EditorPrefs.SetFloat(GizmoColorA, physicsColor.a);
        }

        private static void LoadPrefs()
        {
            showPhysicsGizmos = EditorPrefs.GetBool(ShowGizmoKey, true);

            float r = EditorPrefs.GetFloat(GizmoColorR, physicsColor.r);
            float g = EditorPrefs.GetFloat(GizmoColorG, physicsColor.g);
            float b = EditorPrefs.GetFloat(GizmoColorB, physicsColor.b);
            float a = EditorPrefs.GetFloat(GizmoColorA, physicsColor.a);
            physicsColor = new Color(r, g, b, a);
        }

        public static bool IsShowingGizmos()
        {
            return EditorPrefs.GetBool(ShowGizmoKey, true);
        }

        public static Color GetPhysicsGizmoColor()
        {
            float r = EditorPrefs.GetFloat(GizmoColorR, Color.cyan.r);
            float g = EditorPrefs.GetFloat(GizmoColorG, Color.cyan.g);
            float b = EditorPrefs.GetFloat(GizmoColorB, Color.cyan.b);
            float a = EditorPrefs.GetFloat(GizmoColorA, Color.cyan.a);
            return new Color(r, g, b, a);
        }
    }

    public class PhysicsGizmoDrawer
    {
        public static void OnDrawGizmoPhysicsShape(IPhysicsShape physicsShape)
        {
            switch (true)
            {
                case true when physicsShape is Sphere:
                    OnDrawGizmoSphere((Sphere)physicsShape);
                    break;

                case true when physicsShape is OBB:
                    OnDrawGizmoOBB((OBB)physicsShape);
                    break;

                case true when physicsShape is Capsule:
                    OnDrawGizmoCapsule((Capsule)physicsShape);
                    break;
            }
        }

        private static void OnDrawGizmoSphere(Sphere sphere)
        {
            Gizmos.color = PhysicsGizmoToggleWindow.GetPhysicsGizmoColor();
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }

        private static void OnDrawGizmoOBB(OBB oBB)
        {
            Gizmos.color = PhysicsGizmoToggleWindow.GetPhysicsGizmoColor();

            Vector3 center = oBB.center;
            Vector3 halfSize = oBB.halfSize;

            Vector3 right = oBB.axis[0];
            Vector3 up = oBB.axis[1];
            Vector3 forward = oBB.axis[2];

            Vector3[] vertices = new Vector3[8];

            vertices[0] = center + right * halfSize.x + up * halfSize.y + forward * halfSize.z;
            vertices[1] = center + right * halfSize.x + up * halfSize.y - forward * halfSize.z;
            vertices[2] = center + right * halfSize.x - up * halfSize.y + forward * halfSize.z;
            vertices[3] = center + right * halfSize.x - up * halfSize.y - forward * halfSize.z;
            vertices[4] = center - right * halfSize.x + up * halfSize.y + forward * halfSize.z;
            vertices[5] = center - right * halfSize.x + up * halfSize.y - forward * halfSize.z;
            vertices[6] = center - right * halfSize.x - up * halfSize.y + forward * halfSize.z;
            vertices[7] = center - right * halfSize.x - up * halfSize.y - forward * halfSize.z;

            // 상단면
            Gizmos.DrawLine(vertices[0], vertices[1]);
            Gizmos.DrawLine(vertices[1], vertices[3]);
            Gizmos.DrawLine(vertices[3], vertices[2]);
            Gizmos.DrawLine(vertices[2], vertices[0]);

            // 하단면
            Gizmos.DrawLine(vertices[4], vertices[5]);
            Gizmos.DrawLine(vertices[5], vertices[7]);
            Gizmos.DrawLine(vertices[7], vertices[6]);
            Gizmos.DrawLine(vertices[6], vertices[4]);

            // 수직 연결
            Gizmos.DrawLine(vertices[0], vertices[4]);
            Gizmos.DrawLine(vertices[1], vertices[5]);
            Gizmos.DrawLine(vertices[2], vertices[6]);
            Gizmos.DrawLine(vertices[3], vertices[7]);
        }

        private static void OnDrawGizmoCapsule(Capsule capsule)
        {
            Gizmos.color = PhysicsGizmoToggleWindow.GetPhysicsGizmoColor();

            Vector3 dir = math.normalize((capsule.pointB - capsule.pointA));
            float height = Vector3.Distance(capsule.pointA, capsule.pointB);

            // 양 끝 구체
            Gizmos.DrawWireSphere(capsule.pointA, capsule.radius);
            Gizmos.DrawWireSphere(capsule.pointB, capsule.radius);

            // 중간 몸통 연결 (4방향에서)
            Vector3 up = Vector3.Cross(dir, Vector3.right).normalized;
            if (up == Vector3.zero)
                up = Vector3.Cross(dir, Vector3.forward).normalized;

            Vector3 right = Vector3.Cross(dir, up).normalized;

            up *= capsule.radius;
            right *= capsule.radius;

            Vector3 pointA = capsule.pointA;
            Vector3 pointB = capsule.pointB;

            Gizmos.DrawLine(pointA + up, pointB + up);
            Gizmos.DrawLine(pointA - up, pointB - up);
            Gizmos.DrawLine(pointA + right, pointB + right);
            Gizmos.DrawLine(pointA - right, pointB - right);
        }
    }
}