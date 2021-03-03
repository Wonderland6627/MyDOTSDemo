using System;
using Unity.Entities;

[Serializable]
public struct EntityHealth : IComponentData
{
    public float Value;
}
