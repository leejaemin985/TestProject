using UnityEngine;
using Physics;

public class TestCode : MonoBehaviour
{
    public Transform tr1;
    public Transform tr2;

    public Capsule c;
    
    public OBB ob;


    private bool test = false;

    public void Update()
    {
        c = SweptVolumeCalculator.ComputeSweptSphere(new(tr1), new(tr2));
        test = true;
    }

    private void OnDrawGizmos()
    {
        if (test == false) return;
        PhysicsObject.OnDrawGizmoCapsule(c);
    }
}