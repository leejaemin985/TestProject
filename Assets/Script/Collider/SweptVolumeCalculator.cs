using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Physics
{
    public static class SweptVolumeCalculator
    {
        public static OBB ComputeEncompassingOBB(OBB a, OBB b)
        {
            float3 center = (a.center + b.center) * 0.5f;

            float3 movement = b.center - a.center;
            float movementLen = math.length(movement);
            float3 forward = movementLen > 1e-6f ? movement / movementLen : a.axis[2];

            float3 up = a.axis[1];
            if (math.abs(math.dot(forward, up)) > 0.99f)
            {
                float3 alt = math.abs(math.dot(forward, math.up())) < 0.99f ? math.up() : math.right();
                up = math.normalize(math.cross(forward, alt));
            }

            float3 right = math.normalize(math.cross(up, forward));
            up = math.normalize(math.cross(forward, right));

            float3[] axis = new float3[3] { right, up, forward };

            float3 avgHalf = math.max(a.halfSize, b.halfSize);
            float extraHalfLength = movementLen * 0.5f;
            float3 halfSize = avgHalf;
            halfSize.z += extraHalfLength; // forward รเ

            return new OBB(center, axis, halfSize);
        }

        public static OBB ComputeSweptOBBFromCapsules(Capsule a, Capsule b)
        {
            var points = new List<float3> { a.pointA, a.pointB, b.pointA, b.pointB };

            float radius = math.max(a.radius, b.radius);

            float3 avgCenter = float3.zero;
            foreach (var p in points)
                avgCenter += p;
            avgCenter /= points.Count;

            float3 moveDir = math.normalize(a.center - b.center);
            if (math.lengthsq(moveDir) < 1e-6f)
                moveDir = math.forward();

            float3 up = math.up();
            if (math.dot(up, moveDir) > 0.99f)
                up = math.right();

            float3 right = math.normalize(math.cross(up, moveDir));
            float3 forward = moveDir;
            up = math.normalize(math.cross(forward, right));

            float3x3 toLocal = new float3x3(right, up, forward);
            float3 min = new float3(float.PositiveInfinity);
            float3 max = new float3(float.NegativeInfinity);

            foreach (var p in points)
            {
                float3 local = math.mul(toLocal, p - avgCenter);
                min = math.min(min, local - new float3(radius));
                max = math.max(max, local + new float3(radius));
            }

            float3 localCenter = (min + max) * 0.5f;
            float3 halfSize = (max - min) * 0.5f;
            float3 worldCenter = avgCenter + right * localCenter.x + up * localCenter.y + forward * localCenter.z;

            return new OBB(worldCenter, new float3[] { right, up, forward }, halfSize);
        }

        public static Capsule ComputeSweptSphere(Sphere prev, Sphere curr)
        {
            return new Capsule(prev.center, curr.center, math.max(prev.radius, curr.radius));
        }
    }

}