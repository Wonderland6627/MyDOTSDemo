using System;
using Unity.Entities;

/// <summary>
/// 记录飞行距离 大于200时销毁这个Entity
/// </summary>
[Serializable]
public struct DistanceRecord : IComponentData
{
    public float Time;//飞行时间
    public float Distance;//飞行距离
}
