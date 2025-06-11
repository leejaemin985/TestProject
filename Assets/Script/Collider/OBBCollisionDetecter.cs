using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using UnityEngine;

namespace Physics
{
    public class OBBCollisionDetecter
    {
        public static bool CheckOBBCollision(OBB a, OBB b, out Vector3 contactPoint)
        {
            const float epsilon = 1e-6f;

            Vector3[] A = a.axis;
            Vector3[] B = b.axis;

            float[] aExtents = { a.halfSize.x, a.halfSize.y, a.halfSize.z };
            float[] bExtents = { b.halfSize.x, b.halfSize.y, b.halfSize.z };

            Matrix4x4 R = Matrix4x4.zero;
            Matrix4x4 AbsR = Matrix4x4.zero;

            contactPoint = default;

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
            contactPoint = GetOBBCollisionInfo(a, b);

            return true; // 모든 축에서 분리가 안 됐으면 충돌
        }

        #region Calculate CollisionInfo

        private static Vector3 GetOBBCollisionInfo(OBB a, OBB b)
        {
            List<Vector3> contactPoints = new List<Vector3>();

            // 꼭짓점 배열을 미리 만들어 재사용
            Vector3[] aVerts = a.GetVertices();
            Vector3[] bVerts = b.GetVertices();

            foreach (var v in aVerts)
            {
                if (IsPointInsideOBB(v, b))
                    contactPoints.Add(v);
            }
            foreach (var v in bVerts)
            {
                if (IsPointInsideOBB(v, a))
                    contactPoints.Add(v);
            }

            if (contactPoints.Count == 0)
            {
                // 후보점이 없으면 두 중심점 중간 지점을 기본값으로
                Vector3 midPoint = (a.center + b.center) * 0.5f;
                return midPoint;
            }
            else
            {
                // 후보점들의 평균으로 접촉점 반환
                Vector3 avg = Vector3.zero;
                foreach (var p in contactPoints)
                    avg += p;
                avg /= contactPoints.Count;
                return avg;
            }
        }

        // OBB 내부 점 판정 함수
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
}
