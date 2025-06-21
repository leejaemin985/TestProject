using UnityEngine;
using Physics;

public class TestCode : MonoBehaviour
{
    public Transform c1;
    public Transform c2;

    private Capsule c1s;
    private Capsule c2s;


    private OBB result;

    public void Update()
    {
        c1s = new Capsule(c1);
        c2s = new Capsule(c2);

        result = SweptVolumeCalculator.ComputeSweptOBBFromCapsules(c1s, c2s);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false) return;
        
        PhysicsGizmoDrawer.OnDrawGizmoPhysicsShape(result, Color.red);

        PhysicsGizmoDrawer.OnDrawGizmoPhysicsShape(c1s, Color.cyan);
        PhysicsGizmoDrawer.OnDrawGizmoPhysicsShape(c2s, Color.cyan);
    }
}