using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Physics
{
    public struct Sphere
    {
        public Vector3 center;
        public float radius;

        public Sphere(Transform transform)
        {
            center = transform.position;
            Vector3 scale = transform.lossyScale;
            radius = Mathf.Max(scale.x, scale.y, scale.z) * .5f;
        }

        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public bool ContainsPoint(Vector3 point)
        {
            return Vector3.SqrMagnitude(point - center) <= radius * radius;
        }
    }

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

        public OBB(Vector3 center, Vector3[] axis, Vector3 halfSize)
        {
            this.center = center;
            this.axis = new Vector3[3];
            this.axis[0] = axis[0].normalized;
            this.axis[1] = axis[1].normalized;
            this.axis[2] = axis[2].normalized;
            this.halfSize = halfSize;

            _cachedVertices = new Vector3[8];
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

        public Vector3[] GetVertices()
        {
            if (_cachedVertices == null) _cachedVertices = new Vector3[8];
            UpdateVertices();
            return _cachedVertices;
        }
    }

    public struct Capsule
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public float radius;

        public Capsule(Vector3 pointA, Vector3 pointB, float radius)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.radius = radius;
        }

        public Capsule(Transform capsuleTransform)
        {
            Vector3 center = capsuleTransform.position;

            Vector3 up = capsuleTransform.up; // 방향 벡터 (Y축 기준 회전 포함)
            float height = capsuleTransform.lossyScale.y;
            float radius = Mathf.Max(capsuleTransform.lossyScale.x, capsuleTransform.lossyScale.z) * 0.5f;

            float halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);

            this.pointA = center + up * halfSegment;
            this.pointB = center - up * halfSegment;
            this.radius = radius;
        }

        public Vector3 center => (pointA + pointB) * .5f;

        public float Height => Vector3.Distance(pointA, pointB) + radius * 2;

        public Vector3 Direction => (pointB - pointA).normalized;
    }

    public abstract class PhysicsObject : MonoBehaviour
    {
        public Guid uid { get; private set; } = Guid.NewGuid();

        public bool active;

        public HashSet<Guid> collisionCheckedUIDs { get; private set; }

        public enum PhysicsShapeType
        {
            OBB,
            SPHERE,
            CAPSULE
        }

        public enum PhysicsType
        {
            ATTACK,
            HITABLE
        }

        public abstract PhysicsType physicsType { get; }

        public PhysicsShapeType physicsShapeType;

        private void Start() => Initialized();

        protected virtual void Initialized()
        {
            PhysicsGenerator.Instance.RegisterPhysicsObject(this);
        }

        #region Gizmo
        private void OnDrawGizmos()
        {
            if (!PhysicsGizmoToggleWindow.IsShowingGizmos()) return;

            switch (physicsShapeType)
            {
                case PhysicsShapeType.SPHERE:
                    OnDrawGizmoSphere(new(transform));
                    break;
                case PhysicsShapeType.OBB:
                    OnDrawGizmoOBB(new(transform));
                    break;
                case PhysicsShapeType.CAPSULE:
                    OnDrawGizmoCapsule(new(transform));
                    break;
            }
        }

        public static void OnDrawGizmoSphere(Sphere sphere)
        {
            Gizmos.color = PhysicsGizmoToggleWindow.GetPhysicsGizmoColor();
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }

        public static void OnDrawGizmoOBB(OBB oBB)
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

        public static void OnDrawGizmoCapsule(Capsule capsule)
        {
            Gizmos.color = PhysicsGizmoToggleWindow.GetPhysicsGizmoColor();

            Vector3 dir = (capsule.pointB - capsule.pointA).normalized;
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

            Gizmos.DrawLine(capsule.pointA + up, capsule.pointB + up);
            Gizmos.DrawLine(capsule.pointA - up, capsule.pointB - up);
            Gizmos.DrawLine(capsule.pointA + right, capsule.pointB + right);
            Gizmos.DrawLine(capsule.pointA - right, capsule.pointB - right);
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
}