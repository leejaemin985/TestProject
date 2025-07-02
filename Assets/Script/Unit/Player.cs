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

        private float moveSpeed = 65f;

        private IEnumerator CamSettingHandle = default;

        private NetworkButtons prevInput;

        private void Start() => Initialize();

        private void Initialize()
        {
            StartCamSet();
            hitController.Initialize(playerState, HitMotionTest, attackController.StopAttackMotion);
            animController.Initialize(playerState);
            attackController.Initialized(IsHasInputAuthority, playerState, animController.Play, weapon, hitController.GetPhysicsBox());
            animEventer.Initialize(attackController.SetWeaponActive);
        }

        private bool IsHasInputAuthority()
        {
            return HasInputAuthority;
        }

        private void HitMotionTest(HitInfo hitInfo)
        {
            RPC_OnHitMotionEvent("_HitF", 0, cachedTick);
        }

        [Networked]
        public Vector2 moveAnimTargetDir { get; set; }


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
            animController.SetMoveAnimDirection(moveAnimTargetDir);
        }

        public override void FixedUpdateNetwork()
        {
            cachedTick = Runner.Tick;
            if (GetInput<PlayerInputData>(out var input) == false) return;

            SetPlayerMoveAndRotate(input.Move);

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

            prevInput = input.buttons;
        }


        private void SetPlayerMoveAndRotate(Vector3 inputDir)
        {
            if (playerState.isMotion.state) return;

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
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
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
