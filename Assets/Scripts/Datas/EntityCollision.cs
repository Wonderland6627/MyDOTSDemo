using System;
using Unity.Entities;

[Serializable]
public struct EntityCollision : IComponentData
{
    public float Radius;
}
