using UnityEngine;
using Fusion;

public class UserInputReceiver : RunnerCallbacksBase
{
    protected override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        InputData data = new();

        data.moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift)) data.dash = true;

        if (Input.GetKey(KeyCode.Space)) data.jump = true;

        if (Input.GetKey(KeyCode.Mouse0)) data.attack = true;

        if (Input.GetKey(KeyCode.Mouse1)) data.defense = true;

        input.Set(data);
    }
}
