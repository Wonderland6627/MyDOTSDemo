using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class EnemyConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("小怪移动速度")]
    public float moveSpeed = 2;

    [Header("小怪旋转速度")]
    public float rotateSpeed = 20;

    [Header("小怪生命值")]
    public float enemyHealth = 20;

    [Header("小怪碰撞半径")]
    public float collisionRadius = 1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyTag));
        dstManager.AddComponentData(entity, new NonUniformScale { Value = 1 });

        EntityHealth health = new EntityHealth() { Value = enemyHealth };
        dstManager.AddComponentData(entity, health);

        EntityCollision collision = new EntityCollision() { Radius = collisionRadius };
        dstManager.AddComponentData(entity, collision);

        MoveSpeed move = new MoveSpeed() { Value = moveSpeed };
        dstManager.AddComponentData(entity, move);

        RotateSpeed rotate = new RotateSpeed() { Value = rotateSpeed };
        dstManager.AddComponentData(entity, rotate);

        //EnemyState behaviourState = new EnemyState() { Duration = Random.Range(0.5f, 3f), ChangeTime = Random.Range(0.5f,3f), BehaviourState = EnemyBehaviourState.Idle };
        //dstManager.AddComponentData(entity, behaviourState);
    }
}
