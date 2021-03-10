using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class BulletForwardSystem : JobComponentSystem
{
    [BurstCompile]
    struct MoveJob : IJobForEach<Translation, Rotation, MoveSpeed, EnemyBulletTag>
    {
        [Unity.Collections.ReadOnly]
        public float deltaTime;

        public void Execute(ref Translation pos, ref Rotation rot, ref MoveSpeed moveSpeed, ref EnemyBulletTag tag)
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

        return job.Schedule(this, inputDeps);
    }
}
