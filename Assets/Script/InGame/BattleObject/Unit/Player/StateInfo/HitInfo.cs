using System;
using UnityEngine;
using Fusion;

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
    public Vector3 attackerPos;
    public Vector3 hitPoint;
};