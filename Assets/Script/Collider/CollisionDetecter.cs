using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace Physics
{
    [Serializable]
    public struct CollisionInfo
    {
        public Vector3 contactPointA;
        public Vector3 contactPointB;

        public CollisionInfo(Vector3 contactPointA, Vector3 contactPointB)
        {
            this.contactPointA = contactPointA;
            this.contactPointB = contactPointB;
        }
    }

    public class CollisionDetecter
    {
        public static bool CheckOBBCollision(OBB a, OBB b, out CollisionInfo collisionInfo)
        {
            const float epsilon = 1e-6f;

            Vector3[] A = a.axis;
            Vector3[] B = b.axis;

            float[] aExtents = { a.halfSize.x, a.halfSize.y, a.halfSize.z };
            float[] bExtents = { b.halfSize.x, b.halfSize.y, b.halfSize.z };

            Matrix4x4 R = Matrix4x4.zero;
            Matrix4x4 AbsR = Matrix4x4.zero;

            collisionInfo = default;

            // R[i,j] = Ai dot Bj
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    R[i, j] = Vector3.Dot(A[i], B[j]);
                    AbsR[i, j] = Mathf.Abs(R[i, j]) + epsilon;
                }
            }

            Vector3 t = b.center - a.center;
            // ��ǥ�� A �������� ��ȯ
            t = new Vector3(Vector3.Dot(t, A[0]), Vector3.Dot(t, A[1]), Vector3.Dot(t, A[2]));

            for (int i = 0; i < 3; i++)
            {
                float ra = aExtents[i];
                float rb = bExtents[0] * AbsR[i, 0] + bExtents[1] * AbsR[i, 1] + bExtents[2] * AbsR[i, 2];
                if (Mathf.Abs(t[i]) > ra + rb)
                    return false;
            }

            for (int i = 0; i < 3; i++)
            {
                float ra = aExtents[0] * AbsR[0, i] + aExtents[1] * AbsR[1, i] + aExtents[2] * AbsR[2, i];
                float rb = bExtents[i];
                float tval = Mathf.Abs(t[0] * R[0, i] + t[1] * R[1, i] + t[2] * R[2, i]);
                if (tval > ra + rb)
                    return false;
            }

            // 9 cross product axes (Ai x Bj)
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float ra = aExtents[(i + 1) % 3] * AbsR[(i + 2) % 3, j] + aExtents[(i + 2) % 3] * AbsR[(i + 1) % 3, j];
                    float rb = bExtents[(j + 1) % 3] * AbsR[i, (j + 2) % 3] + bExtents[(j + 2) % 3] * AbsR[i, (j + 1) % 3];
                    float tval = Mathf.Abs(t[(i + 2) % 3] * R[(i + 1) % 3, j] - t[(i + 1) % 3] * R[(i + 2) % 3, j]);
                    if (tval > ra + rb)
                        return false;
                }
            }
            collisionInfo = GetOBBCollisionInfo(a, b);

            return true; // ��� �࿡�� �и��� �� ������ �浹
        }

        #region Calculate CollisionInfo

        private static CollisionInfo GetOBBCollisionInfo(OBB a, OBB b)
        {
            List<Vector3> aPointsInB = new();
            List<Vector3> bPointsInA = new();

            Vector3[] aVerts = a.GetVertices();
            Vector3[] bVerts = b.GetVertices();

            foreach (var v in aVerts)
            {
                if (IsPointInsideOBB(v, b))
                    aPointsInB.Add(v);
            }

            foreach (var v in bVerts)
            {
                if (IsPointInsideOBB(v, a))
                    bPointsInA.Add(v);
            }

            Vector3 contactA = (aPointsInB.Count == 0)
                ? a.center
                : aPointsInB.Aggregate(Vector3.zero, (sum, p) => sum + p) / aPointsInB.Count;

            Vector3 contactB = (bPointsInA.Count == 0)
                ? b.center
                : bPointsInA.Aggregate(Vector3.zero, (sum, p) => sum + p) / bPointsInA.Count;

            return new CollisionInfo(contactA, contactB);
        }

        // OBB ���� �� ���� �Լ�
        static bool IsPointInsideOBB(Vector3 point, OBB obb)
        {
            Vector3 dir = point - obb.center;
            for (int i = 0; i < 3; i++)
            {
                float dist = Vector3.Dot(dir, obb.axis[i]);
                if (dist > obb.halfSize[i] || dist < -obb.halfSize[i])
                    return false;
            }
            return true;
        }

        #endregion
    }

    public class SphereCollisionDetector
    {
        public static bool CheckSphereCollision(Sphere a, Sphere b, out CollisionInfo collisionInfo)
        {
            collisionInfo = default;

            Vector3 direction = b.center - a.center;
            float distanceSq = direction.sqrMagnitude;
            float radiusSum = a.radius + b.radius;

            if (distanceSq > radiusSum * radiusSum)
                return false;

            collisionInfo = GetSphereCollisionInfo(a, b);
            return true;
        }

        private static CollisionInfo GetSphereCollisionInfo(Sphere a, Sphere b)
        {
            Vector3 dir = b.center - a.center;
            float dist = dir.magnitude;

            // �߽��� ���� ���� ��� ó��
            if (dist < 1e-6f)
            {
                // �����ִٰ� ����, �߽� ��ü�� ���������� ó��
                return new CollisionInfo(a.center, b.center);
            }

            Vector3 dirNorm = dir / dist;

            Vector3 contactPointA = a.center + dirNorm * a.radius;
            Vector3 contactPointB = b.center - dirNorm * b.radius;

            return new CollisionInfo(contactPointA, contactPointB);
        }
    }
}
