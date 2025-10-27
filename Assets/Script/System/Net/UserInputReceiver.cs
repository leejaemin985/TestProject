using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInputReceiver : RunnerCallbacksBase
{
    private Vector2 moveDir;
    private bool dash;
    private bool jump;
    private bool attack;
    private bool defense;
    private bool skill;


    protected override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        InputData data = new();

        data.moveDir = moveDir;
        data.dash = dash;
        data.jump = jump;
        data.attack = attack;
        data.defense = defense;
        data.skill = skill;

        input.Set(data);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        dash = context.ReadValue<float>() > 0;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jump = context.ReadValue<float>() > 0;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        attack = context.ReadValue<float>() > 0;
    }

    public void OnDefense(InputAction.CallbackContext context)
    {
        defense = context.ReadValue<float>() > 0;
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        skill = context.ReadValue<float>() > 0;
    }

}
