using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class EnemyMoveSystem : JobComponentSystem
{
    private EntityQuery enemyGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        enemyGroup = GetEntityQuery(typeof(Translation), typeof(Rotation), typeof(MoveSpeed), ComponentType.ReadOnly<EnemyTag>());
    }

    [BurstCompile]
    struct MoveJob : IJobForEach<Translation, Rotation, MoveSpeed>
    {
        public float deltaTime;

        public void Execute(ref Translation pos, ref Rotation rot, ref MoveSpeed moveSpeed)
        {
            pos.Value += moveSpeed.Value * math.forward(rot.Value) * deltaTime;
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MoveJob job = new MoveJob()
        {
            deltaTime = Time.DeltaTime,
        };

        return job.Schedule(enemyGroup, inputDeps);
    }
}
