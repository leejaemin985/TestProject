using Fusion;
using System;

[Serializable]
public struct AttackInfo : INetworkStruct
{
    public AttackMotionType attackMotionType;
}

public enum AttackMotionType
{
    None,
    Dash,
    Air
}