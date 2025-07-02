using Fusion;
using System;

public enum AttackType
{
    NONE,
    GENERIC
}

[Serializable]
public struct HitInfo : INetworkStruct
{
    public float damaged;
    public float weight;
    public AttackType attackType;
};