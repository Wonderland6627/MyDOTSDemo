using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class WeaponConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    public float collisionRadius = 1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(WeaponTag));

        EntityCollision collision = new EntityCollision { Radius = collisionRadius };
        dstManager.AddComponentData(entity, collision);
    }
}
