using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Physics
{
    [Serializable]
    public struct CollisionInfo
    {
        public bool hasCollision;
        public Vector3 contactPointA;
        public Vector3 contactPointB;

        public static CollisionInfo None => new CollisionInfo { hasCollision = false };

        public CollisionInfo(bool hasCollision, Vector3 contactPointA, Vector3 contactPointB)
        {
            this.hasCollision = hasCollision;
            this.contactPointA = contactPointA;
            this.contactPointB = contactPointB;
        }

        public CollisionInfo SwapPoint()
        {
            return new CollisionInfo(hasCollision, contactPointB, contactPointA);
        }
    }

    public static class CollisionDetecter
    {
        public static CollisionInfo CheckCollisionInfo(IPhysicsShape s1, IPhysicsShape s2)
        {
            if (s1.CollisionType == null || s2.CollisionType == null)
            {
                Debug.LogError("[Physics] - One or both shapes have null CollisionType");
                return CollisionInfo.None;
            }

            if (TryGetCollisionHandler(s1.CollisionType, s2.CollisionType, out var handler, out bool swap))
                return swap ? handler(s2, s1).SwapPoint() : handler(s1, s2);

            Debug.LogError($"[Physics] - Not Found CollisionCheckHandler ({s1.GetType().Name}, {s2.GetType().Name})");
            return CollisionInfo.None;
        }

        private static bool TryGetCollisionHandler(Type s1, Type s2, out CollisionCheckHandler handler, out bool swap)
        {
            if (collisionCheckHandlerMap.TryGetValue((s1, s2), out handler))
            {
                swap = false;
                return true;
            }

            if (collisionCheckHandlerMap.TryGetValue((s2, s1), out handler))
            {
                swap = true;
                return true;
            }

            swap = false;
            handler = null;
            return false;
        }



        private delegate CollisionInfo CollisionCheckHandler(IPhysicsShape shapeType1, IPhysicsShape shapeType2);

        private static readonly Dictionary<(Type, Type), CollisionCheckHandler> collisionCheckHandlerMap = new()
        {
            { (typeof(OBB), typeof(OBB)), (s1, s2) => CheckCollision_OBB_OBB((OBB)s1, (OBB)s2) },
            { (typeof(Sphere), typeof(Sphere)), (s1, s2) => CheckCollision_Sphere_Sphere((Sphere)s1, (Sphere)s2) },
            { (typeof(Capsule), typeof(Capsule)), (s1, s2) => CheckCollision_Capsule_Capsule((Capsule)s1, (Capsule)s2) },

            { (typeof(OBB), typeof(Sphere)), (s1, s2) => CheckCollision_OBB_Sphere((OBB)s1, (Sphere)s2) },
            { (typeof(OBB), typeof(Capsule)), (s1, s2) => CheckCollision_OBB_Capsule((OBB)s1, (Capsule)s2) },

            { (typeof(Sphere), typeof(OBB)), (s1, s2) => CheckCollision_Sphere_OBB((Sphere)s1, (OBB)s2) },
            { (typeof(Sphere), typeof(Capsule)), (s1, s2) => CheckCollision_Sphere_Capsule((Sphere)s1, (Capsule)s2) },

            { (typeof(Capsule), typeof(OBB)), (s1, s2) => CheckCollision_Capsule_OBB((Capsule)s1, (OBB)s2) },
            { (typeof(Capsule), typeof(Sphere)), (s1, s2) => CheckCollision_Capsule_Sphere((Capsule)s1, (Sphere)s2) },
        };

        private static CollisionInfo CheckCollision_OBB_OBB(OBB box1, OBB box2)
            => CheckOBBCollision(box1, box2);
        private static CollisionInfo CheckCollision_Sphere_Sphere(Sphere sphere1, Sphere sphere2)
            => CheckSphereCollision(sphere1, sphere2);
        private static CollisionInfo CheckCollision_Capsule_Capsule(Capsule capsule1, Capsule capsule2)
            => CheckCapsuleCollision(capsule1, capsule2);
        private static CollisionInfo CheckCollision_OBB_Sphere(OBB box, Sphere sphere)
            => CheckOBBSphereCollision(box, sphere);
        private static CollisionInfo CheckCollision_Capsule_Sphere(Capsule capsule, Sphere sphere)
            => CheckCapsuleSphereCollision(capsule, sphere);
        private static CollisionInfo CheckCollision_Sphere_OBB(Sphere sphere, OBB box)
            => CheckOBBSphereCollision(box, sphere).SwapPoint();
        private static CollisionInfo CheckCollision_Sphere_Capsule(Sphere sphere, Capsule capsule)
            => CheckCapsuleSphereCollision(capsule, sphere).SwapPoint();
        private static CollisionInfo CheckCollision_Capsule_OBB(Capsule capsule, OBB box)
            => CheckCapsuleOBBCollision(capsule, box);
        private static CollisionInfo CheckCollision_OBB_Capsule(OBB box, Capsule capsule)
            => CheckCapsuleOBBCollision(capsule, box).SwapPoint();

        #region OBB
        private static CollisionInfo CheckOBBCollision(OBB a, OBB b)
        {
            const float epsilon = 1e-6f;

            Vector3[] A = a.axis;
            Vector3[] B = b.axis;

            float[] aExtents = { a.halfSize.x, a.halfSize.y, a.halfSize.z };
            float[] bExtents = { b.halfSize.x, b.halfSize.y, b.halfSize.z };

            Matrix4x4 R = Matrix4x4.zero;
            Matrix4x4 AbsR = Matrix4x4.zero;

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
            // 좌표를 A 기준으로 변환
            t = new Vector3(Vector3.Dot(t, A[0]), Vector3.Dot(t, A[1]), Vector3.Dot(t, A[2]));

            for (int i = 0; i < 3; i++)
            {
                float ra = aExtents[i];
                float rb = bExtents[0] * AbsR[i, 0] + bExtents[1] * AbsR[i, 1] + bExtents[2] * AbsR[i, 2];
                if (Mathf.Abs(t[i]) > ra + rb)
                    return CollisionInfo.None;
            }

            for (int i = 0; i < 3; i++)
            {
                float ra = aExtents[0] * AbsR[0, i] + aExtents[1] * AbsR[1, i] + aExtents[2] * AbsR[2, i];
                float rb = bExtents[i];
                float tval = Mathf.Abs(t[0] * R[0, i] + t[1] * R[1, i] + t[2] * R[2, i]);
                if (tval > ra + rb)
                    return CollisionInfo.None;
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
                        return CollisionInfo.None;
                }
            }
            return GetOBBCollisionInfo(a, b);
        }

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

            return new CollisionInfo(true, contactA, contactB);
        }

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

        #region Sphere
        private static CollisionInfo CheckSphereCollision(Sphere a, Sphere b)
        {
            Vector3 direction = b.center - a.center;
            float distanceSq = direction.sqrMagnitude;
            float radiusSum = a.radius + b.radius;

            if (distanceSq > radiusSum * radiusSum)
                return CollisionInfo.None;

            return GetSphereCollisionInfo(a, b);
        }

        private static CollisionInfo GetSphereCollisionInfo(Sphere a, Sphere b)
        {
            Vector3 dir = b.center - a.center;
            float dist = dir.magnitude;

            if (dist < 1e-6f)
            {
                // 완전 겹쳐있는 경우, 임의 접촉점으로 센터 지정
                return new CollisionInfo(true, a.center, b.center);
            }

            Vector3 dirNorm = dir / dist;
            Vector3 contactPointA = a.center + dirNorm * a.radius;
            Vector3 contactPointB = b.center - dirNorm * b.radius;

            return new CollisionInfo(true, contactPointA, contactPointB);
        }
        #endregion

        #region Capsule
        private static CollisionInfo CheckCapsuleCollision(Capsule a, Capsule b)
        {
            void ClosestPointsBetweenSegments(
                Vector3 p1, Vector3 q1,
                Vector3 p2, Vector3 q2,
                out Vector3 closestPoint1,
                out Vector3 closestPoint2)
            {
                Vector3 d1 = q1 - p1;
                Vector3 d2 = q2 - p2;
                Vector3 r = p1 - p2;

                float a_len = Vector3.Dot(d1, d1);
                float e_len = Vector3.Dot(d2, d2);
                float f = Vector3.Dot(d2, r);

                float s, t;

                if (a_len <= Mathf.Epsilon && e_len <= Mathf.Epsilon)
                {
                    s = t = 0f;
                    closestPoint1 = p1;
                    closestPoint2 = p2;
                    return;
                }

                if (a_len <= Mathf.Epsilon)
                {
                    s = 0f;
                    t = Mathf.Clamp01(f / e_len);
                }
                else
                {
                    float c = Vector3.Dot(d1, r);
                    if (e_len <= Mathf.Epsilon)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-c / a_len);
                    }
                    else
                    {
                        float b = Vector3.Dot(d1, d2);
                        float denom = a_len * e_len - b * b;

                        if (denom != 0f)
                            s = Mathf.Clamp01((b * f - c * e_len) / denom);
                        else
                            s = 0f;

                        t = (b * s + f) / e_len;

                        if (t < 0f)
                        {
                            t = 0f;
                            s = Mathf.Clamp01(-c / a_len);
                        }
                        else if (t > 1f)
                        {
                            t = 1f;
                            s = Mathf.Clamp01((b - c) / a_len);
                        }
                    }
                }

                closestPoint1 = p1 + d1 * s;
                closestPoint2 = p2 + d2 * t;
            }

            ClosestPointsBetweenSegments(a.pointA, a.pointB, b.pointA, b.pointB, out Vector3 pointA, out Vector3 pointB);

            float sqDist = (pointA - pointB).sqrMagnitude;
            float radiusSum = a.radius + b.radius;

            if (sqDist > radiusSum * radiusSum)
                return CollisionInfo.None;

            Vector3 normal;
            if (sqDist < 1e-12f)
            {
                // 거의 같은 위치: 임의 노멀 설정 (예: y축)
                normal = Vector3.up;
            }
            else
            {
                normal = (pointB - pointA).normalized;
            }

            Vector3 contactA = pointA + normal * a.radius;
            Vector3 contactB = pointB - normal * b.radius;

            return new CollisionInfo(true, contactA, contactB);
        }
        #endregion

        #region Both
        private static CollisionInfo CheckOBBSphereCollision(OBB obb, Sphere sphere)
        {
            const float epsilon = 1e-6f;

            Vector3 dir = sphere.center - obb.center;
            Vector3 closestPoint = obb.center;

            // 각 축에 대해 투영 후 clamp
            for (int i = 0; i < 3; i++)
            {
                float projection = Vector3.Dot(dir, obb.axis[i]);
                projection = Mathf.Clamp(projection, -obb.halfSize[i], obb.halfSize[i]);
                closestPoint += obb.axis[i] * projection;
            }

            Vector3 difference = sphere.center - closestPoint;
            float distSqr = difference.sqrMagnitude;
            float radiusSq = sphere.radius * sphere.radius;

            if (distSqr > radiusSq)
                return CollisionInfo.None;

            Vector3 normal = distSqr > epsilon ? difference.normalized : obb.axis[1]; // 작은 거리일 경우 기본 방향

            Vector3 contactPointA = closestPoint;
            Vector3 contactPointB = sphere.center - normal * sphere.radius;

            return new CollisionInfo(true, contactPointA, contactPointB);
        }

        private static CollisionInfo CheckCapsuleSphereCollision(Capsule capsule, Sphere sphere)
        {
            const float epsilon = 1e-6f;

            Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
            {
                Vector3 ab = b - a;
                float abSqrLen = Vector3.Dot(ab, ab);
                if (abSqrLen < epsilon) return a; // 선분이 거의 점일 경우

                float t = Vector3.Dot(point - a, ab) / abSqrLen;
                t = Mathf.Clamp01(t);
                return a + t * ab;
            }

            Vector3 closestPoint = ClosestPointOnSegment(capsule.pointA, capsule.pointB, sphere.center);
            Vector3 dir = sphere.center - closestPoint;
            float distSqr = dir.sqrMagnitude;
            float radiusSum = capsule.radius + sphere.radius;
            float radiusSumSqr = radiusSum * radiusSum;

            if (distSqr > radiusSumSqr)
                return CollisionInfo.None;

            Vector3 normal = distSqr > epsilon ? dir.normalized : Vector3.up; // 거리가 너무 작으면 기본법선

            Vector3 contactPointA = closestPoint + normal * capsule.radius;
            Vector3 contactPointB = sphere.center - normal * sphere.radius;

            return new CollisionInfo(true, contactPointA, contactPointB);
        }

        private static CollisionInfo CheckCapsuleOBBCollision(Capsule capsule, OBB obb)
        {
            const float epsilon = 1e-6f;

            Vector3 ClosestPointOnOBB(Vector3 point, OBB obb)
            {
                Vector3 result = obb.center;
                Vector3 dir = point - obb.center;

                for (int i = 0; i < 3; i++)
                {
                    float dist = Vector3.Dot(dir, obb.axis[i]);
                    dist = Mathf.Clamp(dist, -obb.halfSize[i], obb.halfSize[i]);
                    result += obb.axis[i] * dist;
                }

                return result;
            }

            void ClosestPtSegmentOBB(Vector3 segA, Vector3 segB, OBB obb, out Vector3 closestSegPoint, out Vector3 closestOBBPoint)
            {
                Vector3 segDir = segB - segA;
                float segLength = segDir.magnitude;
                if (segLength < epsilon)
                {
                    // 퇴화된 캡슐, 점 vs OBB 처리
                    closestSegPoint = segA;
                    closestOBBPoint = ClosestPointOnOBB(segA, obb);
                    return;
                }

                segDir /= segLength;

                float minDistSqr = float.MaxValue;
                closestSegPoint = segA;
                closestOBBPoint = ClosestPointOnOBB(segA, obb);

                const int samples = 10;
                for (int i = 0; i <= samples; i++)
                {
                    float t = i / (float)samples;
                    Vector3 pt = Vector3.Lerp(segA, segB, t);
                    Vector3 obbPt = ClosestPointOnOBB(pt, obb);
                    float distSqr = (pt - obbPt).sqrMagnitude;

                    if (distSqr < minDistSqr)
                    {
                        minDistSqr = distSqr;
                        closestSegPoint = pt;
                        closestOBBPoint = obbPt;
                    }
                }
            }

            ClosestPtSegmentOBB(capsule.pointA, capsule.pointB, obb, out Vector3 pointOnCapsule, out Vector3 pointOnOBB);

            Vector3 delta = pointOnOBB - pointOnCapsule;
            float distSqr = delta.sqrMagnitude;
            float radiusSqr = capsule.radius * capsule.radius;

            if (distSqr > radiusSqr)
                return CollisionInfo.None;

            Vector3 normal = distSqr > epsilon ? delta.normalized : Vector3.up;

            Vector3 contactPointA = pointOnOBB;
            Vector3 contactPointB = pointOnCapsule + normal * capsule.radius;

            return new CollisionInfo(true, contactPointA, contactPointB);
        }
        #endregion
    }
}
