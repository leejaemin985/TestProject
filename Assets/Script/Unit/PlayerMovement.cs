using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private SimpleKCC cc;
    [SerializeField] private PlayerAnimController animController;
    [SerializeField] private PlayerCam playerCam;

    private float moveSpeed = 65f;

    private IEnumerator CamSettingHandle = default;

    private void Start()
    {
        if (CamSettingHandle != null) StopCoroutine(CamSettingHandle);
        StartCoroutine(CamSettingHandle = CamSetting());
    }

    [Networked]
    public Vector2 moveAnimTargetDir { get; set; }

    private void Update()
    {
        Debug.Log($"Test - {moveAnimTargetDir}");
        animController.SetMoveAnimDirection(moveAnimTargetDir);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<PlayerInputData>(out var input) == false) return;

        Vector3 moveDir = new Vector3(input.Move.x, 0, input.Move.y).normalized;

        moveAnimTargetDir = new Vector2(moveDir.x, moveDir.z);
        moveDir = Camera.main.transform.TransformDirection(moveDir).normalized;

        if (moveDir.magnitude > .1f)
        {
            cc.SetLookRotation(Quaternion.Slerp(transform.rotation, Camera.main.transform.rotation, 10 * Runner.DeltaTime));
        }
        cc.Move(moveDir * moveSpeed * Runner.DeltaTime);

    }

    private IEnumerator CamSetting()
    {
        if (HasInputAuthority == false) yield break;
        yield return new WaitUntil(() => Runner.IsRunning);

        playerCam.SetCam();
    }
}
