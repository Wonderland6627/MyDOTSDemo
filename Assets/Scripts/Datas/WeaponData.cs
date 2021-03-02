using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct WeaponPosition : IComponentData
{
    public float3 Position;
}

[Serializable]
public struct WeaponRotation : IComponentData
{
    public quaternion Rotation;
}