using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static OBBCollisionDetecter;

public class TestCode : MonoBehaviour
{
    public Transform box1;
    public Transform box2;

    private Vector3[] boxVert = new Vector3[8];
    
    private bool collision = false;

    // Update is called once per frame
    void Update()
    {
        List<Vector3> allPoints = new();

        OBBCollisionDetecter.OBB obbA = new(box1);
        OBBCollisionDetecter.OBB obbB = new(box2);

        allPoints.AddRange(obbA.GetVertices());
        allPoints.AddRange(obbB.GetVertices());

        Vector3 centroid = Vector3.zero;
        foreach (var p in allPoints)
            centroid += p;
        centroid /= allPoints.Count;

        // axis0: 가장 긴 방향
        Vector3 axis0 = (obbB.center - obbA.center).normalized;
        // axis1, axis2: cross product로 직교 축 만들기
        Vector3 arbitrary = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(axis0, arbitrary)) > 0.99f) arbitrary = Vector3.right;
        Vector3 axis1 = Vector3.Cross(axis0, arbitrary).normalized;
        Vector3 axis2 = Vector3.Cross(axis0, axis1).normalized;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var p in allPoints)
        {
            Vector3 local = new Vector3(
                Vector3.Dot(p - centroid, axis0),
                Vector3.Dot(p - centroid, axis1),
                Vector3.Dot(p - centroid, axis2)
            );

            minX = Mathf.Min(minX, local.x);
            maxX = Mathf.Max(maxX, local.x);
            minY = Mathf.Min(minY, local.y);
            maxY = Mathf.Max(maxY, local.y);
            minZ = Mathf.Min(minZ, local.z);
            maxZ = Mathf.Max(maxZ, local.z);
        }
        Vector3 halfSize = new Vector3(
            (maxX - minX) * 0.5f,
            (maxY - minY) * 0.5f,
            (maxZ - minZ) * 0.5f);

        Vector3 obbCenter = centroid
            + axis0 * ((maxX + minX) * 0.5f)
            + axis1 * ((maxY + minY) * 0.5f)
            + axis2 * ((maxZ + minZ) * 0.5f);

        var rotation = Quaternion.LookRotation(axis2, axis1);
        var sweptOBB = new OBBCollisionDetecter.OBB(
            obbCenter,
            rotation.eulerAngles,
            halfSize * 2f // OBB 생성자는 full size 기준
        );

        boxVert = sweptOBB.GetVertices();
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;

        // 윗면 4개
        Gizmos.DrawLine(boxVert[0], boxVert[1]);
        Gizmos.DrawLine(boxVert[1], boxVert[3]);
        Gizmos.DrawLine(boxVert[3], boxVert[2]);
        Gizmos.DrawLine(boxVert[2], boxVert[0]);

        // 아랫면 4개
        Gizmos.DrawLine(boxVert[4], boxVert[5]);
        Gizmos.DrawLine(boxVert[5], boxVert[7]);
        Gizmos.DrawLine(boxVert[7], boxVert[6]);
        Gizmos.DrawLine(boxVert[6], boxVert[4]);

        // 수직 연결
        Gizmos.DrawLine(boxVert[0], boxVert[4]);
        Gizmos.DrawLine(boxVert[1], boxVert[5]);
        Gizmos.DrawLine(boxVert[2], boxVert[6]);
        Gizmos.DrawLine(boxVert[3], boxVert[7]);
    }
}

