using Fusion;
using System;
using UnityEngine;

[Serializable]
public struct MoveInfo : INetworkStruct
{
    public Vector3 moveDir;
    public float velocity;

    public bool wasJumpPressed;
    public bool wasSprint;
}