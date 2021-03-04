using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SkillVfxConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    public float flySpeed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MoveSpeed() { Value = flySpeed });
    }
}
