using UnityEngine;
using Physics;
using UnityEngine.SubsystemsImplementation;

public class TestCode : MonoBehaviour
{
    public AttackBox attackBox;
    public HitBox hitBox;

    public void Start()
    {
        attackBox.Initialize((info)=>Debug.Log($"Test - info center: {info.hitPoint}"));
        hitBox.Initialize();

        attackBox.active = true;
        hitBox.active = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            attackBox.transform.position = new Vector3(10, 0, 0);
        }
    }
}