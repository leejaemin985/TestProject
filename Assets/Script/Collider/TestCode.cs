using UnityEngine;
using Physics;

public class TestCode : MonoBehaviour
{
    public Transform tr1;
    public Transform tr2;

    Capsule c1;
    Capsule c2;

    public OBB ob;

    public void Update()
    {
        c1 = new(tr1);
        c2 = new(tr2);

        ob = SweptVolumeCalculator.ComputeSweptOBBFromCapsules(c1, c2);

    }

    private void OnDrawGizmos()
    {
        if (ob.axis == null || ob.axis.Length != 3) return;


        PhysicsObject.DrawCapsuleGizmo(c1);
        PhysicsObject.DrawCapsuleGizmo(c2);
        DrawOBB(ob, Color.cyan);
    }

    private void DrawOBB(OBB obb, Color color)
    {
        Vector3[] corners = obb.GetVertices();
        Gizmos.color = color;

        // 12개의 모서리 연결
        int[,] edges = new int[12, 2]
        {
            {0,1}, {0,2}, {0,4}, {1,3}, {1,5}, {2,3},
            {2,6}, {3,7}, {4,5}, {4,6}, {5,7}, {6,7}
        };

        for (int i = 0; i < 12; i++)
        {
            Vector3 a = corners[edges[i, 0]];
            Vector3 b = corners[edges[i, 1]];
            Gizmos.DrawLine(a, b);
        }
    }
}