using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PlayerConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    public float playerHealth = 100;
    public float collisionRadius = 1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(PlayerTag));

        EntityHealth health = new EntityHealth() { Value = playerHealth };
        dstManager.AddComponentData(entity, health);

        EntityCollision collision = new EntityCollision() { Radius = collisionRadius };
        dstManager.AddComponentData(entity, collision);
    }
}
