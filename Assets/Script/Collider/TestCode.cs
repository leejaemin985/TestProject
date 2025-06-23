using UnityEngine;
using Physics;
using UnityEngine.SubsystemsImplementation;

public class TestCode : MonoBehaviour
{
    public PhysicsObject physicsObject;

    private IPhysicsShape prevShape;
    private IPhysicsShape currShape;

    private IPhysicsShape sweptShape;


    //public void Start()
    //{
    //    prevShape = physicsObject.currPhysicsShape.CopyClone();
    //    currShape = physicsObject.currPhysicsShape.CopyClone();
    //}

    //public void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.T))
    //    {
    //        prevShape = currShape;
    //        currShape = physicsObject.currPhysicsShape.CopyClone();

    //        sweptShape = prevShape.ComputeSweptVolume(currShape);
    //    }
    //}

    //private void OnDrawGizmos()
    //{
    //    if (Application.isPlaying == false) return;

    //    PhysicsGizmoDrawer.OnDrawGizmoPhysicsShape(prevShape, Color.green);
    //    PhysicsGizmoDrawer.OnDrawGizmoPhysicsShape(currShape, Color.cyan);
    //    PhysicsGizmoDrawer.OnDrawGizmoPhysicsShape(sweptShape, Color.red);
    //}
}