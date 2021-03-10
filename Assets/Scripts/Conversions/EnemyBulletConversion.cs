using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EnemyBulletConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("子弹飞行速度")]
    public float flySpeed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyBulletTag));

        dstManager.AddComponentData(entity, new EntityCollision() { Radius = 1 });
        dstManager.AddComponentData(entity, new DistanceRecord() { Distance = 0 });
        dstManager.AddComponentData(entity, new MoveSpeed() { Value = flySpeed });
    }
}
