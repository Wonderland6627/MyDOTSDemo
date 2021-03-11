using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class WeaponConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("武器碰撞半径")]
    public float collisionRadius = 1;

    [Header("武器威力")]
    public float weaponPower = 100;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(WeaponTag));

        WeaponState weaponState = new WeaponState() { isAttacking = false };
        dstManager.AddComponentData(entity, weaponState);

        EntityCollision collision = new EntityCollision() { Radius = collisionRadius };
        dstManager.AddComponentData(entity, collision);

        dstManager.AddComponentData(entity, new DangerousGoods() { power = weaponPower });
    }
}
