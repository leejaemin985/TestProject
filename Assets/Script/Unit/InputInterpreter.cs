using System;
using UnityEngine;
using Fusion;

public struct InputData : INetworkInput
{
    public Vector2 moveDir;
    public bool dash;
    public bool jump;
    public bool attack;
    public bool defense;

    public bool IsInputEmpty()
    {
        return moveDir.sqrMagnitude < .001f &&
            !dash &&
            !jump &&
            !attack &&
            !defense;
    }
}

public class InputInterpreter
{
    private InputData prev;
    private InputData current;

    public InputData Prev => prev;
    public InputData Current => current;

    public void Update(InputData nextInput)
    {
        prev = current;
        current = nextInput;
    }

    public bool IsSet(Func<InputData, bool> selector) => selector(current);
    public bool WasPressed(Func<InputData, bool> selector) => !selector(prev) && selector(current);
    public bool WasRelease(Func<InputData, bool> selector) => selector(prev) && !selector(current);

}