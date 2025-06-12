using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Physics;

public class TestCode : MonoBehaviour
{
    public enum CollisionMode { OBB, Sphere }
    public CollisionMode collisionMode = CollisionMode.OBB;

    public Transform box1;
    public Transform box2;

    private bool collision = false;
    private CollisionInfo collisionInfo;

    private OBB obbA;
    private OBB obbB;

    private Sphere sphereA;
    private Sphere sphereB;

    private void Update()
    {
        if (box1 == null || box2 == null) return;

        switch (collisionMode)
        {
            case CollisionMode.OBB:
                obbA = new OBB(box1);
                obbB = new OBB(box2);

                collision = CollisionDetecter.CheckOBBCollision(obbA, obbB, out collisionInfo);
                break;

            case CollisionMode.Sphere:
                sphereA = new Sphere(box1.position, GetRadiusFromTransform(box1));
                sphereB = new Sphere(box2.position, GetRadiusFromTransform(box2));

                collision = SphereCollisionDetector.CheckSphereCollision(sphereA, sphereB, out collisionInfo);
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (box1 == null || box2 == null)
            return;

        switch (collisionMode)
        {
            case CollisionMode.OBB:
                OBB a = new OBB(box1);
                OBB b = new OBB(box2);

                DrawOBB(a, Color.cyan);
                DrawOBB(b, Color.cyan);
                break;

            case CollisionMode.Sphere:
                Sphere sA = new Sphere(box1.position, GetRadiusFromTransform(box1));
                Sphere sB = new Sphere(box2.position, GetRadiusFromTransform(box2));

                DrawSphere(sA, Color.cyan);
                DrawSphere(sB, Color.cyan);
                break;
        }

        if (collision)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(collisionInfo.contactPointA, Vector3.one * 0.1f);
            Gizmos.DrawCube(collisionInfo.contactPointB, Vector3.one * 0.1f);
        }
    }

    private void DrawOBB(OBB obb, Color color)
    {
        Vector3[] v = obb.GetVertices();
        Gizmos.color = color;

        // 앞면 사각형 (0,1,3,2)
        Gizmos.DrawLine(v[0], v[1]);
        Gizmos.DrawLine(v[1], v[3]);
        Gizmos.DrawLine(v[3], v[2]);
        Gizmos.DrawLine(v[2], v[0]);

        // 뒷면 사각형 (4,5,7,6)
        Gizmos.DrawLine(v[4], v[5]);
        Gizmos.DrawLine(v[5], v[7]);
        Gizmos.DrawLine(v[7], v[6]);
        Gizmos.DrawLine(v[6], v[4]);

        // 옆면 연결
        Gizmos.DrawLine(v[0], v[4]);
        Gizmos.DrawLine(v[1], v[5]);
        Gizmos.DrawLine(v[2], v[6]);
        Gizmos.DrawLine(v[3], v[7]);
    }

    private void DrawSphere(Sphere sphere, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(sphere.center, sphere.radius);
    }

    private float GetRadiusFromTransform(Transform t)
    {
        // 단순하게 x,y,z 스케일 평균값의 절반을 반지름으로 사용
        Vector3 scale = t.lossyScale;
        float averageScale = (scale.x + scale.y + scale.z) / 3f;
        return averageScale * 0.5f; // 원하는 크기 조절 가능
    }
}
