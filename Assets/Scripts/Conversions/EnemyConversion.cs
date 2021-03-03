using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EnemyConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    public float enemyHealth = 20;
    public float collisionRadius = 1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyTag));

        EntityHealth health = new EntityHealth() { Value = enemyHealth };
        dstManager.AddComponentData(entity, health);

        EntityCollision collision = new EntityCollision() { Radius = collisionRadius };
        dstManager.AddComponentData(entity, collision);
    }
}
