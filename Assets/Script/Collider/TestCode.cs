using UnityEngine;
using Physics;

public class TestCode : MonoBehaviour
{
    public IPhysicsShape[] trs1;
    public IPhysicsShape[] trs2;


    private bool test = false;

    public void Update()
    {
        //c = SweptVolumeCalculator.ComputeSweptSphere();
        //test = true;
    }

    private void OnDrawGizmos()
    {
        //if (test == false) return;
        //PhysicsObject.OnDrawGizmoCapsule(c);
    }
}