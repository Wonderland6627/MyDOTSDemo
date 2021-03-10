using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MathExtension
{
    //AB根据位置碰撞检测
    public static bool CollisionStay(float3 posA, float3 posB, float radius)
    {
        return EntitiesDistance(posA, posB) <= Distance(radius);
    }

    //AB距离
    public static float EntitiesDistance(float3 posA, float3 posB)
    {
        float3 delta = posA - posB;
        float distance = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;

        return distance;
    }

    public static float Distance(float radius)
    {
        return math.pow(radius, 2);
    }
}
