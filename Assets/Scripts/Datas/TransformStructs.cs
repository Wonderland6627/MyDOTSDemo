using System;
using Unity.Entities;

public struct MoveSpeed : IComponentData
{
    public float Value;
}

public struct RotateSpeed : IComponentData
{
    public float Value;
}