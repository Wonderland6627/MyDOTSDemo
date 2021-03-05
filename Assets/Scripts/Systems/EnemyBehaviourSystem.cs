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
public class EnemyBehaviourSystem : ComponentSystem
{
    private Type moveType;
    private Type rotateType;

    protected override void OnUpdate()
    {
        //Entities.
    }

    /*protected override void OnCreate()
    {
        base.OnCreate();

        moveType = typeof(MoveSpeed);
        rotateType = typeof(RotateSpeed);
    }

    struct BehaviourJob : IJobForEachWithEntity<EnemyState>
    {
        public float deltaTime;

        public void Execute(Entity entity, int index, ref EnemyState state)
        {
            state.Duration += deltaTime;
            if (state.Duration >= 5)
            {
                state.Duration = 0;
            }
        }
    }

    static void ChangeState(Entity entity, EnemyBehaviourState state)
    {
        var stateStruct = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<EnemyState>(entity);
        stateStruct.BehaviourState = state;
        World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(entity, stateStruct);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        BehaviourJob behaviourJob = new BehaviourJob()
        {
            deltaTime = Time.DeltaTime,
        };

        return behaviourJob.Schedule(this, inputDeps);
    }*/
}
