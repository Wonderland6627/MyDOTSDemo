using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class EnemyRotateSystem : JobComponentSystem
{
    [BurstCompile]
    struct RotateJob : IJobForEach<Translation, Rotation, RotateSpeed>
    {
        public float deltaTime;
        public float3 targetPos;

        public void Execute(ref Translation pos, ref Rotation rot, ref RotateSpeed rotateSpeed)
        {
            float3 heading = targetPos - pos.Value;
            rot.Value = quaternion.LookRotation(heading, math.up() * deltaTime);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float randX = UnityEngine.Random.Range(0, 2048f);
        float randZ = UnityEngine.Random.Range(0, 2048f);
        float3 randPos = new float3(randX, 0, randZ);
        RotateJob job = new RotateJob()
        {
            targetPos = GameWorld.GetInstance().Player.transform.position,
            deltaTime = Time.DeltaTime,
        };

        return job.Schedule(this, inputDeps);
    }
}
