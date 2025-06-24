using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Physics
{
    public static class SweptVolumeCalculator
    {
        public static OBB ComputeSweptOBBFromOBB(OBB a, OBB b)
        {
            float3 center = (a.center + b.center) * 0.5f;
            float3 movement = b.center - a.center;

            // ✅ 기준 축은 a의 회전 축을 그대로 사용
            float3[] axis = a.axis;

            // ✅ a와 b의 halfSize를 a의 축에 투영해서 크기 결정
            float3 halfSize = new float3();

            for (int i = 0; i < 3; ++i)
            {
                // a, b의 해당 축으로의 half extent
                float extA = math.abs(math.dot(a.axis[i] * a.halfSize[i], axis[i]));
                float extB = math.abs(math.dot(b.axis[i] * b.halfSize[i], axis[i]));
                float extent = math.max(extA, extB);

                // 추가로 movement를 이 축에 투영한 절반만큼 더해줌
                float moveProj = math.abs(math.dot(movement, axis[i])) * 0.5f;

                halfSize[i] = extent + moveProj;
            }

            return new OBB(center, axis, halfSize);
        }

        public static OBB ComputeSweptOBBFromCapsule(Capsule a, Capsule b)
        {
            // --- Convert each capsule to minimal OBB ---
            OBB obbA = CapsuleToOBB(a);
            OBB obbB = CapsuleToOBB(b);

            // --- Compute swept center ---
            float3 center = (obbA.center + obbB.center) * 0.5f;
            float3 movement = obbB.center - obbA.center;
            float movementLen = math.length(movement);

            float3 forward = movementLen > 1e-6f ? movement / movementLen : obbA.axis[2];
            float3 up = obbA.axis[1];

            // Handle degenerate up/forward
            if (math.abs(math.dot(forward, up)) > 0.99f)
            {
                float3 alt = math.abs(math.dot(forward, math.up())) < 0.99f ? math.up() : math.right();
                up = math.normalize(math.cross(forward, alt));
            }

            float3 right = math.normalize(math.cross(up, forward));
            up = math.normalize(math.cross(forward, right));

            float3[] axis = new float3[3] { right, up, forward };

            // --- Compute half size along new axis ---
            float3 localA = ProjectOBBOntoAxis(obbA, axis);
            float3 localB = ProjectOBBOntoAxis(obbB, axis);
            float3 halfSize = math.max(localA, localB);
            halfSize.z += movementLen * 0.5f;

            return new OBB(center, axis, halfSize);

            // --- Converts a capsule to minimal OBB ---
            static OBB CapsuleToOBB(Capsule capsule)
            {
                float3 dir = capsule.pointB - capsule.pointA;
                float height = math.length(dir);

                // fallback 방향 (거의 구에 가까움)
                float3 up = height > 1e-5f ? dir / height : math.up(); // 최소한 up 방향 보장

                float3 arbitrary = math.abs(math.dot(up, math.up())) < 0.99f ? math.up() : math.forward();
                float3 forward = math.normalize(math.cross(up, arbitrary));
                float3 right = math.normalize(math.cross(forward, up));
                forward = math.normalize(math.cross(right, up)); // 보정

                float3[] axis = new float3[3] { right, up, forward };

                float halfHeight = math.max(height * 0.5f, 0f); // 음수 방지
                float3 halfSize = new float3(capsule.radius, halfHeight + capsule.radius, capsule.radius);

                float3 center = (capsule.pointA + capsule.pointB) * 0.5f;

                return new OBB(center, axis, halfSize);
            }

            // --- Projects an OBB onto a new axis set ---
            static float3 ProjectOBBOntoAxis(OBB obb, float3[] targetAxis)
            {
                float3 result = float3.zero;
                for (int i = 0; i < 3; ++i)
                {
                    float3 axis = targetAxis[i];
                    float projection = 0f;
                    for (int j = 0; j < 3; ++j)
                    {
                        projection += math.abs(math.dot(axis, obb.axis[j])) * obb.halfSize[j];
                    }
                    result[i] = projection;
                }
                return result;
            }
        }

        public static Capsule ComputeSweptCapsuleFromSphere(Sphere prev, Sphere curr)
        {
            return new Capsule(prev.center, curr.center, math.max(prev.radius, curr.radius));
        }
    }

}