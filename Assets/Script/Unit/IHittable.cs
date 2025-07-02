using Fusion;
using System;

interface IHittable
{
    public void OnDamaged(HitInfo hitInfo);

}

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