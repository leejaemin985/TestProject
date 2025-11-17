using System;
using UnityEngine;
using Fusion;

public enum HitBehaviorType : byte
{
    NONE,
    GENERIC,
    STUN
}

public enum AttackType : byte
{
    Physical,
    Shockwave
}


[Serializable]
public struct HitInfo : INetworkStruct
{
    public float damaged;
    public float weight;
    public AttackType attackType;
    public HitBehaviorType hitBehaviorType;
    public Vector3 attackerPos;
};