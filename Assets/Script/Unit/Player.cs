using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unit;
using UnityEngine;

namespace Unit
{
    public class PlayerState
    {
        public State isMotion { get; private set; }

        public State isJump { get; private set; }

        public State isAttack { get; private set; }

        public State isHit { get; private set; }

        public PlayerState()
        {
            isMotion = new("isMotion");

            isJump = new("isJump");

            isAttack = new("isAttack");

            isHit = new("isHit");
        }
    }


    public class Player : Unit
    {
        [SerializeField] private SimpleKCC cc;
        [SerializeField] private PlayerAnimController animController;
        [SerializeField] private PlayerCam playerCam;
        [SerializeField] private PlayerAttack attackController;
        [SerializeField] private PlayerHit hitController;
        [SerializeField] private Katana weapon;
        [SerializeField] private PlayerAnimEventer animEventer;

        private int cachedTick = 0;

        private PlayerState playerState = new();

        private Vector3 moveDir = default;
        private Quaternion lookRot = default;

        private float moveSpeed = 65f;
        private float walkSpeed = 65f;
        private float dashSpeed = 150f;
        private float moveSpeedChangedSpeed = 10f;

        private IEnumerator CamSettingHandle = default;

        private NetworkButtons prevInput;

        private void Start() => Initialize();

        private void Initialize()
        {
            StartCamSet();
            hitController.Initialize(playerState, OnHitMotionSync, attackController.StopAttackMotion);
            animController.Initialize(playerState);
            attackController.Initialized(IsHasInputAuthority, playerState, animController.Play, weapon, hitController.GetPhysicsBox());
            animEventer.Initialize(attackController.SetWeaponActive);
        }

        private bool IsHasInputAuthority()
        {
            return HasInputAuthority;
        }

        private void OnHitMotionSync(string hitMotionName, float motionActiveTime)
        {
            RPC_OnHitMotionEvent(hitMotionName, 0, cachedTick);
        }

        [Networked]
        public Vector2 moveAnimTargetDir { get; set; }

        [Networked]
        public float runWeight { get; set; }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_OnAttackEvent(string motionName, float motionActiveTime, int tick, HitInfo[] hitInfos)
        {
            int tickOffset = Runner.Tick - tick;
            float latency = tickOffset * Runner.DeltaTime;

            attackController.SetAttackMotion(motionActiveTime);
            animController.Play(motionName, PlayerAnimController.PlayerAnimLayer.ATTACK, latency);

            attackController.SetHitInfo(hitInfos);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_OnHitMotionEvent(string motionName, float motionActiveTime, int tick)
        {
            int tickOffset = Runner.Tick - tick;
            float latency = tickOffset * Runner.DeltaTime;

            hitController.OnHitMotion(1);
            animController.Play(motionName, PlayerAnimController.PlayerAnimLayer.HIT, latency);
        }

        public override void Render()
        {
            animController.SetMoveAnimDirection(moveAnimTargetDir, runWeight);
        }

        public override void FixedUpdateNetwork()
        {
            cachedTick = Runner.Tick;

            if (GetInput<PlayerInputData>(out var input) == false) return;

            if (playerState.isMotion.state == false)
            {
                SetPlayerMoveAndRotate(input.Move);

                SetMoveSpeed(
                    input.buttons.IsSet(InputButton.Dash) ? dashSpeed : walkSpeed,
                    moveSpeedChangedSpeed);
            }

            if (input.buttons.WasPressed(prevInput, InputButton.LightAttack) == true && attackController.canAttack)
            {
                var attackMotion = attackController.TryAttack(false);
                if (attackMotion.success)
                    RPC_OnAttackEvent(attackMotion.motionName, attackMotion.motionActiveTime, Runner.Tick, attackMotion.hitInfos);
            }
            else if (input.buttons.WasPressed(prevInput, InputButton.HeavyAttack) == true && attackController.canAttack)
            {
                var attackMotion = attackController.TryAttack(true);
                if (attackMotion.success)
                    RPC_OnAttackEvent(attackMotion.motionName, attackMotion.motionActiveTime, Runner.Tick, attackMotion.hitInfos);
            }

            ApplyPlayerMove();

            prevInput = input.buttons;
        }


        private void SetPlayerMoveAndRotate(Vector2 input)
        {
            //애니메이션 방향 설정
            moveAnimTargetDir = input;

            Vector3 inputDir = new Vector3(input.x, 0, input.y);
            
            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();

            if (inputDir.magnitude > 0.1f)
            {
                lookRot = Quaternion.Slerp(transform.rotation, Camera.main.transform.rotation, 10 * Runner.DeltaTime);
            }

            moveDir = inputDir;
        }

        private void ApplyPlayerMove()
        {
            cc.SetLookRotation(lookRot);
            cc.Move(moveDir * moveSpeed * Runner.DeltaTime);
        }

        private void SetMoveSpeed(float targetSpeed, float lerpSpeed)
        {
            if (moveDir.magnitude < .1f) return;

            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, lerpSpeed * Runner.DeltaTime);

            //runWeight값에 따라 이동애니메이션이 설정됩니다.
            float ratio = Mathf.InverseLerp(walkSpeed, dashSpeed, moveSpeed);
            runWeight = Mathf.Lerp(1f, 2f, ratio);
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
}
