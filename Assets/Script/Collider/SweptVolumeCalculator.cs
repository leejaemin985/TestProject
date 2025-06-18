using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physics
{
    public static class SweptVolumeCalculator
    {
        public static OBB ComputeEncompassingOBB(OBB a, OBB b)
        {
            // Step 1: �߽��� �߰� ����
            Vector3 center = (a.center + b.center) * 0.5f;

            // Step 2: �̵� ������ forward�� ���
            Vector3 movement = b.center - a.center;
            Vector3 forward = movement.normalized;
            if (movement.sqrMagnitude < 1e-6f) forward = a.axis[2]; // fallback

            // Step 3: up ���� (a�� up ���)
            Vector3 up = a.axis[1];
            if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
                up = Vector3.Cross(forward, Mathf.Abs(Vector3.Dot(forward, Vector3.up)) < 0.99f ? Vector3.up : Vector3.right).normalized;

            // Step 4: ���� �� ����
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            Vector3[] axis = new Vector3[3] { right, up, forward };

            // Step 5: halfSize ���
            Vector3 avgHalf = Vector3.Max(a.halfSize, b.halfSize); // �浹 ���� ���� ���� �ִ� ���
            float extraHalfLength = movement.magnitude * 0.5f;      // forward ���� ���� ����
            Vector3 halfSize = new Vector3(avgHalf.x, avgHalf.y, avgHalf.z);
            halfSize.z += extraHalfLength; // forward �� ���� (axis[2])

            return new(center, axis, halfSize);
        }

        public static OBB ComputeSweptOBBFromCapsules(Capsule a, Capsule b)
        {
            List<Vector3> points = new List<Vector3>
            {
                a.pointA, a.pointB,
                b.pointA, b.pointB
            };

            // �ִ� ������ ��� (�� �� ū ��)
            float radius = Mathf.Max(a.radius, b.radius);

            // �߽� ���
            Vector3 avgCenter = Vector3.zero;
            foreach (var p in points)
                avgCenter += p;
            avgCenter /= points.Count;

            // �̵� ���� ���� ȸ��
            Vector3 moveDir = (a.center - b.center).normalized;
            if (moveDir == Vector3.zero)
                moveDir = Vector3.forward; // ���� ��ġ�� �⺻��

            // ���� ��ǥ�� ����
            Vector3 up = Vector3.up;
            if (Vector3.Dot(up, moveDir) > 0.99f) // �����ϸ� �ٸ� �� ���
                up = Vector3.right;

            Vector3 right = Vector3.Cross(up, moveDir).normalized;
            Vector3 forward = moveDir;
            up = Vector3.Cross(forward, right).normalized;

            Vector3[] axis = new Vector3[] { right, up, forward };

            // ��� ���� local space�� ����
            Matrix4x4 toLocal = Matrix4x4.identity;
            toLocal.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
            toLocal.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
            toLocal.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0));
            toLocal.SetColumn(3, new Vector4(0, 0, 0, 1));
            toLocal = toLocal.transpose;

            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;

            foreach (var p in points)
            {
                Vector3 local = toLocal.MultiplyPoint3x4(p - avgCenter);
                min = Vector3.Min(min, local - Vector3.one * radius);
                max = Vector3.Max(max, local + Vector3.one * radius);
            }

            // ���� OBB ����
            Vector3 localCenter = (min + max) * 0.5f;
            Vector3 halfSize = (max - min) * 0.5f;
            Vector3 worldCenter = avgCenter +
                right * localCenter.x + up * localCenter.y + forward * localCenter.z;

            return new OBB(worldCenter, axis, halfSize);
        }

        public static Capsule ComputeSweptSphere(Sphere prev, Sphere curr)
        {
            return new Capsule(prev.center, curr.center, Mathf.Max(prev.radius, curr.radius));
        }
    }
}