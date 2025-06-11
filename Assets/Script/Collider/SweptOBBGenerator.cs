using System.Collections.Generic;
using UnityEngine;
using Physics;

[RequireComponent(typeof(BoxCollider))]
public class SweptOBBGenerator : MonoBehaviour
{
    private BoxCollider boxCollider;

    private Vector3 prevPosition;
    private Quaternion prevRotation;

    private OBB sweptOBB;
    private Vector3[] sweptVertices = new Vector3[8];

    private bool isFirstUpdate = true;
    private int frameCounter = 0;
    private int updateInterval => 1;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        CacheInitialState();
        Application.targetFrameRate = 60;
    }

    void OnEnable()
    {
        CacheInitialState();
        isFirstUpdate = true;
        frameCounter = 0;
    }

    void CacheInitialState()
    {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
    }

    void Update()
    {
        frameCounter++;

        if (isFirstUpdate || frameCounter >= updateInterval)
        {
            OBB obbNow = new OBB(transform.position, transform.rotation.eulerAngles, GetWorldSize());
            OBB obbPrev = new OBB(prevPosition, prevRotation.eulerAngles, GetWorldSize());

            sweptOBB = CreateSweptOBB(obbPrev, obbNow);
            sweptVertices = sweptOBB.GetVertices();

            // 갱신
            prevPosition = transform.position;
            prevRotation = transform.rotation;
            frameCounter = 0;
            isFirstUpdate = false;
        }
    }

    private Vector3 GetWorldSize()
    {
        return Vector3.Scale(boxCollider.size, transform.lossyScale);
    }

    private OBB CreateSweptOBB(OBB obbA, OBB obbB)
    {
        List<Vector3> points = new List<Vector3>();
        points.AddRange(obbA.GetVertices());
        points.AddRange(obbB.GetVertices());

        Vector3 centroid = Vector3.zero;
        foreach (var p in points)
            centroid += p;
        centroid /= points.Count;

        Vector3 axis0 = (obbB.center - obbA.center).normalized;
        if (axis0.magnitude < 1e-4f) axis0 = obbA.axis[0];

        Vector3 arbitrary = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(axis0, arbitrary)) > 0.99f) arbitrary = Vector3.right;
        Vector3 axis1 = Vector3.Cross(axis0, arbitrary).normalized;
        Vector3 axis2 = Vector3.Cross(axis0, axis1).normalized;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var p in points)
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

        Vector3 center = centroid
            + axis0 * ((maxX + minX) * 0.5f)
            + axis1 * ((maxY + minY) * 0.5f)
            + axis2 * ((maxZ + minZ) * 0.5f);

        Quaternion rotation = Quaternion.LookRotation(axis2, axis1);

        return new OBB(center, rotation.eulerAngles, halfSize * 2f);
    }

    void OnDrawGizmos()
    {
        if (sweptVertices == null || sweptVertices.Length != 8) return;

        Gizmos.color = Color.cyan;

        // 윗면
        Gizmos.DrawLine(sweptVertices[0], sweptVertices[1]);
        Gizmos.DrawLine(sweptVertices[1], sweptVertices[3]);
        Gizmos.DrawLine(sweptVertices[3], sweptVertices[2]);
        Gizmos.DrawLine(sweptVertices[2], sweptVertices[0]);

        // 아랫면
        Gizmos.DrawLine(sweptVertices[4], sweptVertices[5]);
        Gizmos.DrawLine(sweptVertices[5], sweptVertices[7]);
        Gizmos.DrawLine(sweptVertices[7], sweptVertices[6]);
        Gizmos.DrawLine(sweptVertices[6], sweptVertices[4]);

        // 수직 연결
        Gizmos.DrawLine(sweptVertices[0], sweptVertices[4]);
        Gizmos.DrawLine(sweptVertices[1], sweptVertices[5]);
        Gizmos.DrawLine(sweptVertices[2], sweptVertices[6]);
        Gizmos.DrawLine(sweptVertices[3], sweptVertices[7]);
    }
}
