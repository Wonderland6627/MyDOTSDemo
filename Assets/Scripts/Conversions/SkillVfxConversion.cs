using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SkillVfxConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("飞行速度")]
    public float flySpeed = 10;

    [Header("技能威力")]
    public float skillPower = 100;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(SkillVFXTag));

        dstManager.AddComponentData(entity, new EntityCollision() { Radius = 1 });
        dstManager.AddComponentData(entity, new DistanceRecord() { Distance = 0 });
        dstManager.AddComponentData(entity, new DangerousGoods() { power = skillPower });
        dstManager.AddComponentData(entity, new MoveSpeed() { Value = flySpeed });
    }
}
