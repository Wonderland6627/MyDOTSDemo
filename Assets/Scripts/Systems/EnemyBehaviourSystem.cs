using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

/// <summary>
/// 通过增删ComponentData控制行为
/// </summary>
public class EnemyBehaviourSystem : JobComponentSystem
{
    private Type moveType;
    private Type rotateType;

    private static EntityManager entityManager;

    protected override void OnCreate()
    {
        base.OnCreate();

        moveType = typeof(MoveSpeed);
        rotateType = typeof(RotateSpeed);

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    struct BehaviourJob : IJobForEachWithEntity<EnemyState>
    {
        public float deltaTime;

        public void Execute(Entity entity, int index, ref EnemyState state)
        {
            state.Duration += deltaTime;
            if (state.Duration > 5)
            {
                if (!entityManager.HasComponent<MoveSpeed>(entity))
                {
                    entityManager.AddComponent<MoveSpeed>(entity);
                }
            }
            else
            {
                if (entityManager.HasComponent<MoveSpeed>(entity))
                {
                    entityManager.RemoveComponent<MoveSpeed>(entity);
                }
            }
        }
    }

    static void ChangeState(Entity entity, EnemyBehaviourState state)
    {
        var stateStruct = entityManager.GetComponentData<EnemyState>(entity);
        stateStruct.BehaviourState = state;
        entityManager.SetComponentData(entity, stateStruct);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        BehaviourJob behaviourJob = new BehaviourJob()
        {
            deltaTime = Time.DeltaTime,
        };

        return behaviourJob.Schedule(this, inputDeps);
    }
}
