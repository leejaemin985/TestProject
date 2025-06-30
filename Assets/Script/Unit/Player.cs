using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class Player : NetworkBehaviour
{
    public enum PlayerAnimState
    {
        IDLE,
        ATTACK,
        HIT,
        DEAD
    }

    public struct PlayerAnimData : INetworkStruct
    {
        public int startTick;
        public PlayerAnimState state;
        public int motionNumber;
    }

    [SerializeField] private SimpleKCC cc;
    [SerializeField] private PlayerAnimController animController;
    [SerializeField] private PlayerCam playerCam;
    [SerializeField] private PlayerAttack attackController;

    private float moveSpeed = 65f;

    private IEnumerator CamSettingHandle = default;

    private NetworkButtons prevInput;

    private void Start()
    {
        StartCamSet();
        attackController.Initialized(
            (animName, normalizedTime) => animController.Play(animName, 0, normalizedTime),
            () => animController.Play("_Idle"));
    }

    [Networked]
    public Vector2 moveAnimTargetDir { get; set; }

    private float GetNormalizedAnimationTime(int startTick, float animDuration)
    {
        int tickOffset = Runner.Tick - startTick;
        float pressedTime = tickOffset * Runner.DeltaTime + 1f;
        return Mathf.Clamp01(pressedTime / animDuration);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetAnimation(PlayerAnimData animData)
    {
        if (animData.state == PlayerAnimState.ATTACK)
        {
            float normalizedTime = GetNormalizedAnimationTime(animData.startTick, Runner.Tick);
            Debug.Log($"Test - {normalizedTime}");

            int tickOffset = Runner.Tick - animData.startTick;
            float pressedTime = tickOffset * Runner.DeltaTime;
            normalizedTime = Mathf.Clamp01(pressedTime / attackController.attackMotionDuration[animData.motionNumber]);
            Debug.Log($"Test - {normalizedTime}");
            attackController.SetAttackMotion(animData.motionNumber, normalizedTime);
            if (HasInputAuthority == true)
            {
                attackController.SetCanAttack(false);
            }
        }
    }


    public override void Render()
    {
        animController.SetMoveAnimDirection(moveAnimTargetDir);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<PlayerInputData>(out var input) == false) return;

        SetPlayerMoveAndRotate(input.Move);

        if (input.buttons.WasPressed(prevInput, InputButton.Attack) == true && attackController.canAttack)
        {
            RPC_SetAnimation(new()
            {
                startTick = Runner.Tick,
                state = PlayerAnimState.ATTACK,
                motionNumber = attackController.TryGetAttack(),
            });
        }

        prevInput = input.buttons;
    }


    private void SetPlayerMoveAndRotate(Vector3 inputDir)
    {
        Vector3 moveDir = new Vector3(inputDir.x, 0, inputDir.y).normalized;

        moveAnimTargetDir = new Vector2(moveDir.x, moveDir.z);

        moveDir = Camera.main.transform.TransformDirection(moveDir).normalized;
        if (moveDir.magnitude > .1f)
        {
            cc.SetLookRotation(Quaternion.Slerp(transform.rotation, Camera.main.transform.rotation, 10 * Runner.DeltaTime));
        }
        cc.Move(moveDir * moveSpeed * Runner.DeltaTime);
    }

    #region Cam
    private void StartCamSet()
    {
        if (CamSettingHandle != null) StopCoroutine(CamSettingHandle);
        StartCoroutine(CamSettingHandle = CamSetting());
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator CamSetting()
    {
        if (HasInputAuthority == false) yield break;
        yield return new WaitUntil(() => Runner.IsRunning);

        playerCam.SetCam();
    }
    #endregion
}
