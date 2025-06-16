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
            // Step 1: �߽��� �߰� ����
            Vector3 center = (a.center + b.center) * 0.5f;

            // Step 2: �̵� ������ forward�� ���
            Vector3 movement = b.center - a.center;
            Vector3 forward = movement.normalized;
            if (movement.sqrMagnitude < 1e-6f) forward = a.axis[2]; // fallback

            // Step 3: up ���� (a�� up ���)
            Vector3 up = a.axis[1];
            if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
                up = Vector3.Cross(forward, Vector3.right).normalized;

            // Step 4: ���� �� ����
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            Vector3[] axis = new Vector3[3] { right, up, forward };

            // Step 5: halfSize ���
            Vector3 avgHalf = Vector3.Max(a.halfSize, b.halfSize); // �浹 ���� ���� ���� �ִ� ���
            float extraHalfLength = movement.magnitude * 0.5f;      // forward ���� ���� ����
            Vector3 halfSize = new Vector3(avgHalf.x, avgHalf.y, avgHalf.z);
            halfSize.z += extraHalfLength; // forward �� ���� (axis[2])

            return new OBB
            {
                center = center,
                axis = axis,
                halfSize = halfSize
            };
        }

        public static OBB ComputeSweptOBBFromCapsules(Capsule a, Capsule b)
        {
            // 1. �߽��� �̵� ����� �߰���
            Vector3 center = (a.center + b.center) * 0.5f;

            // 2. �̵� ������ forward�� ���
            Vector3 movement = b.center - a.center;
            Vector3 forward = movement.normalized;
            if (movement.sqrMagnitude < 1e-6f)
                forward = a.Direction; // ���� �̵� ������ ĸ�� ����

            // 3. up ������ ĸ���� ���� ���
            Vector3 up = a.Direction;
            if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
                up = Vector3.up;

            // 4. ����ȭ �� ����
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            Vector3[] axis = new Vector3[3] { right, up, forward };

            // 5. Radius�� �� ĸ�� �� �ִ� ���
            float radius = Mathf.Max(a.radius, b.radius);

            // 6. ���̴� �� ĸ�� �� �ִ�
            float maxHeight = Mathf.Max(a.Height, b.Height);

            // 7. OBB halfSize ���
            Vector3 halfSize = new Vector3(
                radius,
                radius,
                maxHeight * 0.5f + movement.magnitude * 0.5f // �������� �÷���
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