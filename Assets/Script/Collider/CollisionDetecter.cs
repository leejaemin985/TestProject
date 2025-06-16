using System;
using System.Linq;
using System.Collections.Generic;

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
        public static bool CheckCollision(OBB box1, OBB box2, out CollisionInfo collisionInfo)
            => CheckOBBCollision(box1, box2, out collisionInfo);

        public static bool CheckCollision(Sphere sphere1, Sphere sphere2, out CollisionInfo collisionInfo)
            => CheckSphereCollision(sphere1, sphere2, out collisionInfo);
        
        public static bool CheckCollision(Capsule capsule1, Capsule capsule2, out CollisionInfo collisionInfo)
            => CheckCapsuleCollision(capsule1, capsule2, out collisionInfo);


        public static bool CheckCollision(OBB box, Sphere sphere, out CollisionInfo collisionInfo)
            => CheckOBBSphereCollision(box, sphere, out collisionInfo);
        
        public static bool CheckCollision(Capsule capsule, Sphere sphere, out CollisionInfo collisionInfo)
            => CheckCapsuleSphereCollision(capsule, sphere, out collisionInfo);

        public static bool CheckCollision(Sphere sphere, OBB box, out CollisionInfo collisionInfo)
        {
            bool ret = CheckOBBSphereCollision(box, sphere, out collisionInfo);
            var temp = collisionInfo.contactPointB;
            collisionInfo.contactPointB = collisionInfo.contactPointA;
            collisionInfo.contactPointA = temp;
            return ret;
        }

        public static bool CheckCollision(Sphere sphere, Capsule capsule, out CollisionInfo collisionInfo)
        {
            bool ret = CheckCapsuleSphereCollision(capsule, sphere, out collisionInfo);

            var temp = collisionInfo.contactPointB;
            collisionInfo.contactPointB = collisionInfo.contactPointA;
            collisionInfo.contactPointA = temp;

            return ret;
        }

        public static bool CheckCollision(Capsule capsule, OBB box, out CollisionInfo collisionInfo)
            => CheckCapsuleOBBCollision(capsule, box, out collisionInfo);

        public static bool CheckCollision(OBB box, Capsule capsule, out CollisionInfo collisionInfo)
        {
            bool ret = CheckCapsuleOBBCollision(capsule, box, out collisionInfo);
            var temp = collisionInfo.contactPointB;
            collisionInfo.contactPointB = collisionInfo.contactPointA;
            collisionInfo.contactPointA = temp;
            return ret;
        }

        #region OBB
        private static bool CheckOBBCollision(OBB a, OBB b, out CollisionInfo collisionInfo)
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
            // 좌표를 A 기준으로 변환
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

            return true; // 모든 축에서 분리가 안 됐으면 충돌
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

            return new CollisionInfo(contactA, contactB);
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
        private static bool CheckSphereCollision(Sphere a, Sphere b, out CollisionInfo collisionInfo)
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

            // 중심이 거의 같을 경우 처리
            if (dist < 1e-6f)
            {
                // 겹쳐있다고 보고, 중심 자체를 접촉점으로 처리
                return new CollisionInfo(a.center, b.center);
            }

            Vector3 dirNorm = dir / dist;

            Vector3 contactPointA = a.center + dirNorm * a.radius;
            Vector3 contactPointB = b.center - dirNorm * b.radius;

            return new CollisionInfo(contactPointA, contactPointB);
        }
        #endregion

        #region Capsule
        private static bool CheckCapsuleCollision(Capsule a, Capsule b, out CollisionInfo collisionInfo)
        {
            collisionInfo = default;

            void ClosestPointsBetweenSegments(
            Vector3 p1, Vector3 q1,
            Vector3 p2, Vector3 q2,
            out Vector3 closestPoint1,
            out Vector3 closestPoint2)
            {
                Vector3 d1 = q1 - p1; // 방향1
                Vector3 d2 = q2 - p2; // 방향2
                Vector3 r = p1 - p2;

                float a = Vector3.Dot(d1, d1); // 제곱 길이1
                float e = Vector3.Dot(d2, d2); // 제곱 길이2
                float f = Vector3.Dot(d2, r);

                float s, t;

                if (a <= Mathf.Epsilon && e <= Mathf.Epsilon)
                {
                    // 둘 다 점
                    s = t = 0f;
                    closestPoint1 = p1;
                    closestPoint2 = p2;
                    return;
                }

                if (a <= Mathf.Epsilon)
                {
                    s = 0f;
                    t = Mathf.Clamp01(f / e);
                }
                else
                {
                    float c = Vector3.Dot(d1, r);
                    if (e <= Mathf.Epsilon)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-c / a);
                    }
                    else
                    {
                        float b = Vector3.Dot(d1, d2);
                        float denom = a * e - b * b;

                        if (denom != 0f)
                        {
                            s = Mathf.Clamp01((b * f - c * e) / denom);
                        }
                        else
                        {
                            s = 0f;
                        }

                        t = (b * s + f) / e;

                        if (t < 0f)
                        {
                            t = 0f;
                            s = Mathf.Clamp01(-c / a);
                        }
                        else if (t > 1f)
                        {
                            t = 1f;
                            s = Mathf.Clamp01((b - c) / a);
                        }
                    }
                }

                closestPoint1 = p1 + d1 * s;
                closestPoint2 = p2 + d2 * t;
            }

            ClosestPointsBetweenSegments(
                a.pointA, a.pointB,
                b.pointA, b.pointB,
                out Vector3 pointA,
                out Vector3 pointB
            );

            float sqDist = (pointA - pointB).sqrMagnitude;
            float radiusSum = a.radius + b.radius;

            if (sqDist > radiusSum * radiusSum)
                return false;

            Vector3 normal = (pointB - pointA).normalized;
            Vector3 contactA = pointA + normal * a.radius;
            Vector3 contactB = pointB - normal * b.radius;

            collisionInfo = new CollisionInfo(contactA, contactB);
            return true;
        }
        #endregion

        #region Both
        private static bool CheckOBBSphereCollision(OBB obb, Sphere sphere, out CollisionInfo collisionInfo)
        {
            collisionInfo = default;

            Vector3 dir = sphere.center - obb.center;
            Vector3 closestPoint = obb.center;

            // 각 축에 대해 투영 후 clamp
            for (int i = 0; i < 3; i++)
            {
                float dist = Vector3.Dot(dir, obb.axis[i]);
                dist = Mathf.Clamp(dist, -obb.halfSize[i], obb.halfSize[i]);
                closestPoint += obb.axis[i] * dist;
            }

            Vector3 difference = sphere.center - closestPoint;
            float distanceSqr = difference.sqrMagnitude;
            float radius = sphere.radius;

            if (distanceSqr > radius * radius)
                return false;

            // 접촉 지점 계산
            Vector3 normal = difference.normalized;
            Vector3 contactPointA = closestPoint;
            Vector3 contactPointB = sphere.center - normal * radius;

            collisionInfo = new CollisionInfo(contactPointA, contactPointB);
            return true;
        }

        private static bool CheckCapsuleSphereCollision(Capsule capsule, Sphere sphere, out CollisionInfo collisionInfo)
        {
            collisionInfo = default;

            Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
            {
                Vector3 ab = b - a;
                float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
                t = Mathf.Clamp01(t);
                return a + t * ab;
            }

            // 캡슐의 선분에서 구 중심까지의 최근접점
            Vector3 closestPoint = ClosestPointOnSegment(capsule.pointA, capsule.pointB, sphere.center);

            Vector3 dir = sphere.center - closestPoint;
            float distanceSqr = dir.sqrMagnitude;
            float radiusSum = capsule.radius + sphere.radius;

            if (distanceSqr > radiusSum * radiusSum)
                return false;

            // 정규화된 법선 방향 (충돌 시)
            Vector3 normal = dir.normalized;

            // 접촉 지점 계산
            Vector3 contactPointA = closestPoint + normal * capsule.radius;
            Vector3 contactPointB = sphere.center - normal * sphere.radius;

            collisionInfo = new CollisionInfo(contactPointA, contactPointB);
            return true;
        }

        private static bool CheckCapsuleOBBCollision(Capsule capsule, OBB obb, out CollisionInfo collisionInfo)
        {
            collisionInfo = default;

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
                if (segLength < 1e-6f)
                {
                    // 퇴화된 캡슐, 그냥 점 vs OBB
                    closestSegPoint = segA;
                    closestOBBPoint = ClosestPointOnOBB(segA, obb);
                    return;
                }

                segDir /= segLength;

                float bestT = 0f;
                float minDistSqr = float.MaxValue;

                // 초기값으로 segA 기준 할당 (for문에서 갱신 실패 시도 방지)
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
                        bestT = t;
                        closestSegPoint = pt;
                        closestOBBPoint = obbPt;
                    }
                }
            }

            // 캡슐의 중심 선분에 대해 OBB와 가장 가까운 점을 찾음
            ClosestPtSegmentOBB(capsule.pointA, capsule.pointB, obb, out Vector3 pointOnCapsule, out Vector3 pointOnOBB);

            Vector3 delta = pointOnOBB - pointOnCapsule;
            float distSqr = delta.sqrMagnitude;

            if (distSqr > capsule.radius * capsule.radius)
                return false;

            Vector3 normal = distSqr > 1e-6f ? delta.normalized : Vector3.up;
            Vector3 contactPointA = pointOnOBB;
            Vector3 contactPointB = pointOnCapsule + normal * capsule.radius;

            collisionInfo = new CollisionInfo(contactPointA, contactPointB);
            return true;
        }
        #endregion
    }
}
