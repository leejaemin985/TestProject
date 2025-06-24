using UnityEngine;
using Physics;
using UnityEngine.SubsystemsImplementation;

public class TestCode : MonoBehaviour
{
    public AttackBox attackBox;
    public HitBox[] hitBox;

    public void Start()
    {
        attackBox.Initialize((info)=>Debug.Log($"Test - info center: {info.hitObject.gameObject.name}"));
        attackBox.SetActive(true);

        foreach (var obb in hitBox)
        {
            obb.Initialize();
            obb.SetActive(true);
        }
    }

    Vector3 pos1 = new Vector3(1, 0, 0) * 10;
    Vector3 pos2 = new Vector3(1, 0, 0) * -10;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            attackBox.transform.position = attackBox.transform.position == pos1 ? pos2 : pos1;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            attackBox.SetActive(false);
            attackBox.SetActive(true);
        }
    }
}