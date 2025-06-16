using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Physics
{
    public static class SweptVolumeCalculator
    {
        public static OBB ComputeEncompassingOBB(OBB a, OBB b)
        {
            // Step 1: 중심은 중간 지점
            Vector3 center = (a.center + b.center) * 0.5f;

            // Step 2: 이동 방향을 forward로 사용
            Vector3 movement = b.center - a.center;
            Vector3 forward = movement.normalized;
            if (movement.sqrMagnitude < 1e-6f) forward = a.axis[2]; // fallback

            // Step 3: up 방향 (a의 up 사용)
            Vector3 up = a.axis[1];
            if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
                up = Vector3.Cross(forward, Vector3.right).normalized;

            // Step 4: 직교 축 구성
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            Vector3[] axis = new Vector3[3] { right, up, forward };

            // Step 5: halfSize 계산
            Vector3 avgHalf = Vector3.Max(a.halfSize, b.halfSize); // 충돌 누락 방지 위해 최댓값 사용
            float extraHalfLength = movement.magnitude * 0.5f;      // forward 방향 길이 증가
            Vector3 halfSize = new Vector3(avgHalf.x, avgHalf.y, avgHalf.z);
            halfSize.z += extraHalfLength; // forward 축 기준 (axis[2])

            return new OBB
            {
                center = center,
                axis = axis,
                halfSize = halfSize
            };
        }

        public static OBB ComputeSweptOBBFromCapsules(Capsule a, Capsule b)
        {
            // 1. 중심은 이동 경로의 중간점
            Vector3 center = (a.center + b.center) * 0.5f;

            // 2. 이동 방향을 forward로 사용
            Vector3 movement = b.center - a.center;
            Vector3 forward = movement.normalized;
            if (movement.sqrMagnitude < 1e-6f)
                forward = a.Direction; // 거의 이동 없으면 캡슐 방향

            // 3. up 방향은 캡슐의 방향 사용
            Vector3 up = a.Direction;
            if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
                up = Vector3.up;

            // 4. 직교화 축 구성
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            Vector3[] axis = new Vector3[3] { right, up, forward };

            // 5. Radius는 두 캡슐 중 최대 사용
            float radius = Mathf.Max(a.radius, b.radius);

            // 6. 높이는 두 캡슐 중 최대
            float maxHeight = Mathf.Max(a.Height, b.Height);

            // 7. OBB halfSize 계산
            Vector3 halfSize = new Vector3(
                radius,
                radius,
                maxHeight * 0.5f + movement.magnitude * 0.5f // 방향으로 늘려줌
            );

            return new OBB
            {
                center = center,
                axis = axis,
                halfSize = halfSize
            };
        }

        public static Capsule ComputeSweptSphere(Sphere prev, Sphere curr)
        {
            return default;
        }
    }
}